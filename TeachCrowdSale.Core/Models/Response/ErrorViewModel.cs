﻿namespace TeachCrowdSale.Core.Models.Response
{
    /// <summary>
    /// Error view model
    /// </summary>
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
