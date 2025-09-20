using Core.Dtos;
using Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class ChangeRequestService
    {
        private readonly ChangeRequestRepository _repo = new ChangeRequestRepository();



        public ChangeRequests RequestDetail(string requestid, string dep_c)
            => _repo.ViewDetailRequest(requestid, dep_c);

        public String ProcessAction(string requestid, string dep_c, string action, string user)
          => _repo.ProcessAction(requestid, dep_c, action, user);
    }
}
