﻿using ArtHubBO.Entities;

namespace ArtHubRepository.Interface
{
    public interface ISubscriberRepository : IBaseRepository<Subscriber> 
    {
        public int GetTotalSubscribers();
    }
}
