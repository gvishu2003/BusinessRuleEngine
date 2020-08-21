using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BRE.Interfaces;
using BRE.Repository;


namespace BRE
{
    public class BusinessEngineService : IBusinessRule
    {
        private List<IRules> rules = new List<IRules>();
        private List<string> actions = new List<string>();
        public static BusinessEngineService LoadFromString(string ruleInString)
        {
            var lines = ruleInString.Split('\n');
            var brs = new BusinessEngineService();
            var ruleSelection = ExtractRules(lines);
            brs.rules.AddRange(ruleSelection);

            var selection = ExtractActions(lines);
            brs.actions.AddRange(selection);
            return brs;
        }

        private static IEnumerable<string> ExtractActions(IEnumerable<string> lines)
        {
            var selection = lines.SkipWhile(x => !x.Contains("actions:"));
            selection = selection.Skip(1);
            selection = selection.Select(x => x.Trim().Replace("- ", ""));
            return selection;
        }

        private static IEnumerable<IRules> ExtractRules(IEnumerable<string> lines)
        {
            var extracted = ExtractLines(lines);

            var selection = extracted.Select<string, IRules>(x =>
            {
                if (x.StartsWith("membership upgrade"))
                {
                    return new MembershipUpgradeRepository();
                }
                else if (x.StartsWith("membership request"))
                {
                    var extractedType = x.Replace("membership request", "").Trim();
                    extractedType = extractedType.Substring(0, 1).ToUpper() + extractedType.Substring(1, extractedType.Length - 1);
                    var membershipType = (MembershipType)Enum.Parse(typeof(MembershipType), extractedType);
                    return new MembershipRepository(membershipType);
                }
                else if (x.StartsWith("commission"))
                {
                    return new CommissionRepository();
                }
                else if (x.StartsWith("video"))
                {
                    var title = x.Replace("video", "").Trim();
                    return new ProductRepository("video", title);
                }
                else if (x.StartsWith("physical product"))
                {
                    return new PhysicalProductRepository();
                }
                else
                {
                    throw new Exception();
                }
            });

            return selection;
        }

        private static IEnumerable<string> ExtractLines(IEnumerable<string> lines)
        {
            var selection = lines.SkipWhile(x => !x.Contains("rules:"));
            selection = selection.Skip(1);
            selection = selection.TakeWhile(x => !x.Contains("actions:"));
            selection = selection.Select(x => x.Trim().Replace("- ", ""));
            return selection;
        }

        public bool ShouldApply(IGetOrder purchaseOrder)
        {
            return rules.All(x => x.ShouldApply(purchaseOrder));
        }

        public void Apply(IOrderFunctionalities purchaseOrder)
        {
             actions.ForEach(x => 
                {
                if (x.Contains("membership activate books"))
                {
                    purchaseOrder.AddMembership(MembershipType.Books);
                }
                else if (x.Contains("add video"))
                {
                    purchaseOrder.AddVideo(x.Replace("add video", "").Trim());
                }
                else if (x.Contains("membership activate upgrade"))
                {
                    purchaseOrder.UpgradeMembership();
                }
                else if (x.Contains("create shipping slip"))
                {
                    purchaseOrder.CreateShippingSlip();
                }
                else if (x.Contains("generate comission"))
                {
                    purchaseOrder.GenerateCommission();
                }
            });
        }
    }
}

