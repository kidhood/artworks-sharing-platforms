﻿using ArtHubBO.DTO;
using ArtHubBO.Payload;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtHubService.Interface
{
    
    public interface ITopCreatorService
    {
        public Task<PageResult<TopCreatorDTO>> GetTopCreator(SearchTopCreatorDTO search);

    }
}
