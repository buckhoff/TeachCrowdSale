// web3.js - Web3 integration for connecting wallets and purchasing tokens
document.addEventListener('DOMContentLoaded', function() {
    // Web3 Provider and Contract Variables
    let web3;
    let accounts = [];
    let currentAccount;
    let presaleContract;
    let tokenContract;
    let networkId;
    let chainConnected = false;

    // Contract Addresses (will be fetched from API)
    let contractAddresses = {
        presale: '',
        token: '',
        paymentToken: ''
    };

    // Connect Wallet Button
    const connectWalletBtn = document.querySelector('.btn-connect-wallet');
    if (connectWalletBtn) {
        connectWalletBtn.addEventListener('click', connectWallet);
    }

    // Purchase Form
    const purchaseForm = document.getElementById('purchaseForm');
    if (purchaseForm) {
        purchaseForm.addEventListener('submit', handlePurchase);
    }

    // Initialize Web3
    async function initWeb3() {
        // Check if MetaMask is installed
        if (window.ethereum) {
            try {
                // Modern dapp browsers
                web3 = new Web3(window.ethereum);

                // Check if already authorized
                accounts = await window.ethereum.request({ method: 'eth_accounts' });

                if (accounts.length > 0) {
                    currentAccount = accounts[0];
                    updateUIWithAccount(currentAccount);
                }

                // Setup event listeners
                window.ethereum.on('accountsChanged', handleAccountsChanged);
                window.ethereum.on('chainChanged', handleChainChanged);

                // Get network ID
                networkId = await web3.eth.net.getId();

                // Determine if we're on the correct network
                checkNetwork(networkId);

                // Fetch contract addresses
                await fetchContractAddresses();

                // Initialize contracts if on correct network
                if (chainConnected) {
                    initContracts();
                }

                return true;
            } catch (error) {
                console.error("Error initializing Web3:", error);
                showError("Error initializing Web3 connection. Please try again.");
                return false;
            }
        } else {
            showError("Please install MetaMask or another Web3 provider to interact with the presale.");
            return false;
        }
    }

    // Connect Wallet
    async function connectWallet() {
        if (!web3) {
            const initialized = await initWeb3();
            if (!initialized) return;
        }

        try {
            // Request account access
            accounts = await window.ethereum.request({ method: 'eth_requestAccounts' });
            currentAccount = accounts[0];

            // Update UI with connected account
            updateUIWithAccount(currentAccount);

            // Check if we need to switch networks
            if (!chainConnected) {
                await switchToCorrectNetwork();
            }

            // Initialize contracts if not already done
            if (contractAddresses.presale && !presaleContract) {
                initContracts();
            }

            // Update purchase form if it exists
            updatePurchaseForm();
        } catch (error) {
            console.error("Error connecting wallet:", error);
            showError("Error connecting wallet. Please try again.");
        }
    }

    // Handle Accounts Changed
    function handleAccountsChanged(accounts) {
        if (accounts.length === 0) {
            // User disconnected
            currentAccount = null;
            updateUIWithAccount(null);
        } else if (accounts[0] !== currentAccount) {
            // Account changed
            currentAccount = accounts[0];
            updateUIWithAccount(currentAccount);

            // Update purchase form if it exists
            updatePurchaseForm();
        }
    }

    // Handle Chain Changed
    async function handleChainChanged(chainId) {
        // Reload the page on chain change as recommended by MetaMask
        window.location.reload();
    }

    // Check if connected to correct network
    function checkNetwork(networkId) {
        // Define correct network IDs (Ethereum Mainnet or Polygon)
        // For Polygon Mainnet: 137, For Polygon Mumbai Testnet: 80001
        const correctNetworks = [1, 137]; // Ethereum Mainnet, Polygon Mainnet

        chainConnected = correctNetworks.includes(networkId);

        // Update UI based on network status
        updateNetworkUI(chainConnected, networkId);

        return chainConnected;
    }

    // Switch to correct network
    async function switchToCorrectNetwork() {
        try {
            // Try to switch to Polygon Mainnet (chainId: 0x89)
            await window.ethereum.request({
                method: 'wallet_switchEthereumChain',
                params: [{ chainId: '0x89' }], // 137 in hexadecimal
            });

            // Update network connection status
            networkId = await web3.eth.net.getId();
            chainConnected = checkNetwork(networkId);

            return chainConnected;
        } catch (switchError) {
            // If the error code is 4902, the chain hasn't been added
            if (switchError.code === 4902) {
                try {
                    // Add Polygon network
                    await window.ethereum.request({
                        method: 'wallet_addEthereumChain',
                        params: [{
                            chainId: '0x89',
                            chainName: 'Polygon Mainnet',
                            nativeCurrency: {
                                name: 'MATIC',
                                symbol: 'MATIC',
                                decimals: 18
                            },
                            rpcUrls: ['https://polygon-rpc.com/'],
                            blockExplorerUrls: ['https://polygonscan.com/']
                        }],
                    });

                    // Update network connection status
                    networkId = await web3.eth.net.getId();
                    chainConnected = checkNetwork(networkId);

                    return chainConnected;
                } catch (addError) {
                    console.error("Error adding Polygon network:", addError);
                    showError("Error adding Polygon network. Please add it manually in MetaMask.");
                    return false;
                }
            } else {
                console.error("Error switching networks:", switchError);
                showError("Error switching networks. Please manually switch to Polygon in MetaMask.");
                return false;
            }
        }
    }

    // Fetch Contract Addresses
    async function fetchContractAddresses() {
        try {
            const response = await fetch('/api/contracts');
            const data = await response.json();

            contractAddresses.presale = data.presaleAddress;
            contractAddresses.token = data.tokenAddress;
            contractAddresses.paymentToken = data.paymentTokenAddress;

            return true;
        } catch (error) {
            console.error("Error fetching contract addresses:", error);
            showError("Error fetching contract information. Please try again later.");
            return false;
        }
    }

    // Initialize Contracts
    function initContracts() {
        // Initialize PresaleContract
        if (contractAddresses.presale && web3) {
            const presaleABI = []; // Will be fetched or imported
            presaleContract = new web3.eth.Contract(presaleABI, contractAddresses.presale);
        }

        // Initialize TokenContract
        if (contractAddresses.token && web3) {
            const tokenABI = []; // Will be fetched or imported
            tokenContract = new web3.eth.Contract(tokenABI, contractAddresses.token);
        }
    }

    // Handle Token Purchase
    async function handlePurchase(event) {
        event.preventDefault();

        // Ensure wallet is connected
        if (!currentAccount) {
            showError("Please connect your wallet first.");
            return;
        }

        // Ensure on correct network
        if (!chainConnected) {
            const switched = await switchToCorrectNetwork();
            if (!switched) return;
        }

        // Get form data
        const tierId = parseInt(document.getElementById('tier').value);
        const amount = document.getElementById('amount').value;

        // Convert amount to correct units (USDC has 6 decimals)
        const amountInUnits = web3.utils.toWei(amount, 'mwei'); // 6 decimals for USDC

        try {
            // First approve payment token spending
            await approvePaymentToken(amountInUnits);

            // Then purchase tokens
            await purchaseTokens(tierId, amountInUnits);

            // Show success message
            showSuccess("Purchase successful! Check your wallet for tokens after the presale ends.");

            // Update UI
            fetchUserPurchases();
        } catch (error) {
            console.error("Error during purchase:", error);
            showError("Error during purchase: " + error.message);
        }
    }

    // Approve Payment Token Spending
    async function approvePaymentToken(amount) {
        // Get payment token contract
        const paymentTokenABI = []; // Will be fetched or imported
        const paymentTokenContract = new web3.eth.Contract(paymentTokenABI, contractAddresses.paymentToken);

        // Check current allowance
        const allowance = await paymentTokenContract.methods.allowance(currentAccount, contractAddresses.presale).call();

        // If allowance is less than amount, approve more
        if (new web3.utils.BN(allowance).lt(new web3.utils.BN(amount))) {
            await paymentTokenContract.methods.approve(contractAddresses.presale, amount).send({
                from: currentAccount
            });
        }
    }

    // Purchase Tokens
    async function purchaseTokens(tierId, amount) {
        await presaleContract.methods.purchase(tierId, amount).send({
            from: currentAccount
        });
    }

    // Update UI With Account
    function updateUIWithAccount(account) {
        const connectWalletBtn = document.querySelector('.btn-connect-wallet');
        if (connectWalletBtn) {
            if (account) {
                // Show truncated address
                const truncatedAccount = account.substring(0, 6) + '...' + account.substring(account.length - 4);
                connectWalletBtn.textContent = truncatedAccount;
                connectWalletBtn.classList.add('connected');
            } else {
                connectWalletBtn.textContent = 'Connect Wallet';
                connectWalletBtn.classList.remove('connected');
            }
        }

        // Show/hide purchase form
        const purchaseForm = document.getElementById('purchaseForm');
        if (purchaseForm) {
            if (account && chainConnected) {
                purchaseForm.classList.remove('d-none');
                document.getElementById('connectWalletMessage').classList.add('d-none');
            } else {
                purchaseForm.classList.add('d-none');
                document.getElementById('connectWalletMessage').classList.remove('d-none');
            }
        }

        // Update user purchases if account is connected
        if (account) {
            fetchUserPurchases();
        }
    }

    // Update Network UI
    function updateNetworkUI(connected, networkId) {
        const networkStatus = document.getElementById('networkStatus');
        if (networkStatus) {
            if (connected) {
                let networkName = "Unknown";

                // Map network ID to name
                switch (networkId) {
                    case 1:
                        networkName = "Ethereum Mainnet";
                        break;
                    case 137:
                        networkName = "Polygon Mainnet";
                        break;
                    case 80001:
                        networkName = "Polygon Mumbai Testnet";
                        break;
                }

                networkStatus.textContent = `Connected to ${networkName}`;
                networkStatus.className = 'text-success';
            } else {
                networkStatus.textContent = "Please connect to Polygon Mainnet";
                networkStatus.className = 'text-danger';
            }
        }

        // Enable/disable purchase form based on network connection
        const purchaseForm = document.getElementById('purchaseForm');
        if (purchaseForm && currentAccount) {
            if (connected) {
                purchaseForm.classList.remove('d-none');
                document.getElementById('connectWalletMessage').classList.add('d-none');
            } else {
                purchaseForm.classList.add('d-none');
                document.getElementById('wrongNetworkMessage').classList.remove('d-none');
            }
        }
    }

    // Update Purchase Form
    async function updatePurchaseForm() {
        if (!currentAccount || !presaleContract) return;

        try {
            // Get current tier
            const currentTierId = await presaleContract.methods.getCurrentTier().call();

            // Set current tier in dropdown
            const tierSelect = document.getElementById('tier');
            if (tierSelect) {
                tierSelect.value = currentTierId;
            }

            // Get tier details
            const tierDetails = await presaleContract.methods.tiers(currentTierId).call();

            // Update min/max purchase amounts
            const amountInput = document.getElementById('amount');
            if (amountInput) {
                // Convert from wei to USDC (6 decimals)
                const minPurchase = web3.utils.fromWei(tierDetails.minPurchase, 'mwei');
                const maxPurchase = web3.utils.fromWei(tierDetails.maxPurchase, 'mwei');

                amountInput.setAttribute('min', minPurchase);
                amountInput.setAttribute('max', maxPurchase);
                amountInput.setAttribute('placeholder', `Min: ${minPurchase} USDC, Max: ${maxPurchase} USDC`);
            }

            // Update price display
            const priceDisplay = document.getElementById('tokenPrice');
            if (priceDisplay) {
                // Convert from wei to USDC (6 decimals)
                const price = web3.utils.fromWei(tierDetails.price, 'mwei');
                priceDisplay.textContent = price + ' USDC';
            }
        } catch (error) {
            console.error("Error updating purchase form:", error);
        }
    }

    // Fetch User Purchases
    async function fetchUserPurchases() {
        if (!currentAccount || !presaleContract) return;

        try {
            // Get user purchase info
            const purchase = await presaleContract.methods.purchases(currentAccount).call();

            // Update user purchase display
            const purchasedTokens = document.getElementById('purchasedTokens');
            if (purchasedTokens) {
                // Convert from wei
                const tokens = web3.utils.fromWei(purchase.tokens, 'ether');
                purchasedTokens.textContent = numberWithCommas(tokens) + ' TEACH';
            }

            const purchasedAmount = document.getElementById('purchasedAmount');
            if (purchasedAmount) {
                // Convert from wei to USDC (6 decimals)
                const amount = web3.utils.fromWei(purchase.usdAmount, 'mwei');
                purchasedAmount.textContent = numberWithCommas(amount) + ' USDC';
            }

            // Show purchase history if available
            const purchaseHistory = document.getElementById('purchaseHistory');
            if (purchaseHistory && purchase.tokens > 0) {
                purchaseHistory.classList.remove('d-none');
            }
        } catch (error) {
            console.error("Error fetching user purchases:", error);
        }
    }

    // Show Error Message
    function showError(message) {
        const errorAlert = document.getElementById('errorAlert');
        if (errorAlert) {
            errorAlert.textContent = message;
            errorAlert.classList.remove('d-none');

            // Hide after 5 seconds
            setTimeout(() => {
                errorAlert.classList.add('d-none');
            }, 5000);
        } else {
            alert(message);
        }
    }

    // Show Success Message
    function showSuccess(message) {
        const successAlert = document.getElementById('successAlert');
        if (successAlert) {
            successAlert.textContent = message;
            successAlert.classList.remove('d-none');

            // Hide after 5 seconds
            setTimeout(() => {
                successAlert.classList.add('d-none');
            }, 5000);
        } else {
            alert(message);
        }
    }

    // Format numbers with commas for better readability
    function numberWithCommas(x) {
        return x.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
    }

    // Initialize Web3 on page load
    initWeb3();
});