using Microsoft.AspNetCore.Mvc;
using NetFwTypeLib;
using System.Net;
using System.Text.Json.Serialization;

namespace WindowsFirewallFeed.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FirewallController : ControllerBase
    {
        private readonly ILogger<FirewallController> _logger;
        private INetFwPolicy2 internal_fwPolicy2 = (INetFwPolicy2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));
        public FirewallController(ILogger<FirewallController> logger)
        {
            _logger = logger;
        }

        [HttpGet("rule/scope")]
        public ContentResult GetRuleScopeByName([FromQuery] string Name, [FromQuery] FirewallScopeLocation Location = FirewallScopeLocation.REMOTE, [FromQuery] FirewallRuleMatching Matching = FirewallRuleMatching.EQUALS)
        {
            ContentResult _return = Content(String.Empty);
            if (!String.IsNullOrWhiteSpace(Name))
            {
                List<INetFwRule> rules = new List<INetFwRule>();
                if (Matching == FirewallRuleMatching.EQUALS)
                {
                    try
                    {
                        INetFwRule _rule = internal_fwPolicy2.Rules.Item(Name);
                        rules.Add(_rule);
                    }
                    catch
                    {
                        rules.Clear();
                    }
                }
                else 
                {
                    foreach(INetFwRule rule in internal_fwPolicy2.Rules)
                    {
                        switch (Matching)
                        {
                            case FirewallRuleMatching.BEGINS:
                                if (rule.Name.StartsWith(Name))
                                    rules.Add(rule);
                                break;
                            case FirewallRuleMatching.ENDS:
                                if (rule.Name.EndsWith(Name))
                                    rules.Add(rule);
                                break;
                            case FirewallRuleMatching.CONTAINS:
                                if (rule.Name.Contains(Name))
                                    rules.Add(rule);
                                break;
                        }
                    }
                }
                if (rules.Count > 0)
                {
                    List<string> ipTable = new List<string>(); 
                    foreach(INetFwRule rule in rules)
                    {
                        switch(Location)
                        {
                            case FirewallScopeLocation.REMOTE:
                                if (rule.RemoteAddresses == "*")
                                    continue;
                                else
                                    ipTable.AddRange(rule.RemoteAddresses.Split(","));
                                break;
                            case FirewallScopeLocation.LOCAL:
                                if (rule.LocalAddresses == "*")
                                    continue;
                                else
                                    ipTable.AddRange(rule.LocalAddresses.Split(","));
                                break;
                        }
                    }
                    _return = Content(String.Join("\r\n",ConvertMaskToCIDR(ipTable)));
                }
            }
            else
            {
                _return = Content("No rule name specified.");
            }
            return _return;
        }

        private List<string> ConvertMaskToCIDR(List<string> ipTable)
        {
            for(int i=0;i<ipTable.Count;i++)
            {
                string ipaddress = ipTable[i].Split("/")[0];
                string netmask = ipTable[i].Split("/")[1];
                UInt32 cidr = SubnetToCIDR(netmask);
                if (cidr == 32)
                    ipTable[i] = ipaddress;
                else
                    ipTable[i] = $"{ipaddress}/{cidr}";
            }
            return ipTable;
        }
        private UInt32 SubnetToCIDR(string subnetStr)
        {
            IPAddress subnetAddress = IPAddress.Parse(subnetStr);
            byte[] ipParts = subnetAddress.GetAddressBytes();
            UInt32 subnet = 16777216 * Convert.ToUInt32(ipParts[0]) + 65536 * Convert.ToUInt32(ipParts[1]) + 256 * Convert.ToUInt32(ipParts[2]) + Convert.ToUInt32(ipParts[3]);
            UInt32 mask = 0x80000000;
            UInt32 subnetConsecutiveOnes = 0;
            for (int i = 0; i < 32; i++)
            {
                if (!(mask & subnet).Equals(mask)) break;

                subnetConsecutiveOnes++;
                mask = mask >> 1;
            }
            return subnetConsecutiveOnes;
        }

    }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum FirewallScopeLocation
    {
        LOCAL,
        REMOTE
    }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum FirewallRuleMatching
    {
        BEGINS,
        ENDS,
        CONTAINS,
        EQUALS
    }
}