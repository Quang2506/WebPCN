using System.Collections.Generic;
using Core.Dtos;
using Data.Repositories;

namespace Services
{
    public class WorkFlowService
    {
        private readonly WorkFlowRepository _repo = new WorkFlowRepository();

        public IEnumerable<WorkFlow> GetJobStatusHistory(string changeRequestID, string dep_c)
            => _repo.GetJobStatusHistory(changeRequestID, dep_c);
    }
}