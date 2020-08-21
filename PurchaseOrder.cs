using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BRE.Interfaces;
using BRE.Repository;

namespace BRE
{
    public class PurchaseOrder : IOrderProcessor, IOrderFunctionalities, IOrder, IGetOrder, IGetShippingSlip
    {
        
        List<IBusinessRule> rules = new List<IBusinessRule>();
        List<string> books = new List<string>();
        List<string> videos = new List<string>();
        IGetCustomer getCustomer;
        ICustomer customer;

        IGetCommission getCommission;
        ICommission commission;
        double total;

        public IEnumerable<string> Request { get; private set; }
        public PurchaseOrder(string request, IGetCustomer getCustomer, IGetCommission getCommission)
        {
            Request = request.Split('\n');
            Books = books;
            Video = videos;
            this.getCustomer = getCustomer;
            this.getCommission = getCommission;
            ProcessRequest();
        }

        private void ProcessRequest()
        {
            GetTotal();
            GetCustomer();
            GetAgentCommission();
        }

        private void GetTotal()
        {
            var requestTotal = Request.FirstOrDefault(x => x.StartsWith("Total:"));
            if (!string.IsNullOrWhiteSpace(requestTotal))
            {
                requestTotal = requestTotal.Replace("Total:", "").Trim();
                total = int.Parse(requestTotal);
            }
        }

        private void GetCustomer()
        {

            var requestCustomerId = Request.FirstOrDefault(x => x.StartsWith("Customer:"));
            if (!string.IsNullOrWhiteSpace(requestCustomerId))
            {
                requestCustomerId = requestCustomerId.Replace("Customer:", "").Trim();
                var customerId = int.Parse(requestCustomerId);
                customer = getCustomer.GetCustomer(customerId);
            }
        }

        private void GetAgentCommission()
        {

            var agentName = Request.FirstOrDefault(x => x.StartsWith("Commision:"));
            if (!string.IsNullOrWhiteSpace(agentName))
            {
                agentName = agentName.Replace("Agent:", "").Trim();
                commission = getCommission.GetCommision(agentName);
            }
        }

        public IEnumerable<string> Books { get; private set; }

        public IEnumerable<string> Video { get; private set; }

        public void AddRules(List<IBusinessRule> rulesToAdd)
        {
            this.rules.AddRange(rulesToAdd);
        }

        public void Process()
        {
            foreach (var rule in rules)
            {
                if (rule.ShouldApply(this))
                {
                    rule.Apply(this);
                }
            }
        }

        public IOrderFunctionalities AddMembership(MembershipType membershipType)
        {
            customer.AddMembership(membershipType);
            return this;
        }

        public IOrderFunctionalities UpgradeMembership()
        {
            customer.UpgradeMembership();
            return this;
        }

        public IOrderFunctionalities CreateShippingSlip()
        {
            Slip = new ShippingSlip();
            return this;
        }

        public IOrderFunctionalities GenerateCommission()
        {
            commission.GenerateCommission(total, 5);
            return this;
        }

        public IOrderFunctionalities AddBook(string title)
        {
            books.Add(title);
            return this;
        }

        public IOrderFunctionalities AddVideo(string title)
        {
            videos.Add(title);
            return this;
        }

        public ShippingSlip Slip { get; private set; }
    }
}

