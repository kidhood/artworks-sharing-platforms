﻿using ArtHubBO.Entities;
using ArtHubDAO.Interface;
using ArtHubRepository.Interface;

namespace ArtHubRepository.Repository
{
    public class SubscriberRepository : BaseRepository<Subscriber>, ISubscriberRepository
    {
        public SubscriberRepository(IBaseDAO<Subscriber> baseDAO) : base(baseDAO)
        {
        }

        public int GetTotalSubscribers()
        {
            return this.DbSet.Count();
        }

    }
}
