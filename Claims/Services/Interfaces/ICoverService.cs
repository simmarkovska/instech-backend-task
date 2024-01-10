﻿using Claims.Enums;
using Claims.Models;
using Microsoft.AspNetCore.Mvc;

namespace Claims.Services.Interfaces
{
    public interface ICoverService
    {
        Task<IEnumerable<Cover>> GetCoversAsync();
        Task<Cover> GetCoverAsync(string id);
        IActionResult DateValidation(Cover cover);
        Task AddItemAsync(Cover cover);
        Task DeleteItemAsync(string id);
        decimal ComputePremium(DateOnly startDate, DateOnly endDate, CoverType coverType);
    }
}