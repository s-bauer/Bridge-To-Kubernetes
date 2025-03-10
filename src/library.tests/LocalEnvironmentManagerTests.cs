﻿// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Autofac;
using Microsoft.BridgeToKubernetes.Common.Models;
using Microsoft.BridgeToKubernetes.Common.Models.Settings;
using Microsoft.BridgeToKubernetes.Library.Connect;
using Microsoft.BridgeToKubernetes.Library.Models;
using Microsoft.BridgeToKubernetes.TestHelpers;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using static Microsoft.BridgeToKubernetes.Common.Constants;

namespace Microsoft.BridgeToKubernetes.Library.Tests
{
    public class LocalEnvironmentManagerTests : TestsBase
    {
        private readonly ILocalEnvironmentManager _localEnvironmentManager;

        public LocalEnvironmentManagerTests() =>
            _localEnvironmentManager =
                _autoFake.Resolve<LocalEnvironmentManager>(new NamedParameter(
                    "useKubernetesServiceEnvironmentVariables",
                    true));

        public static IEnumerable<object[]> TestData()
        {
            // single basic endpoint
            yield return new object[]
            {
                new[] {
                    new EndpointInfo
                    {
                        DnsName = "foo",
                        LocalIP = System.Net.IPAddress.Parse("127.0.0.1"),
                        Ports = new[] { new PortPair(5050, 80) }
                    }
                },
                new Dictionary<string, string>
                {
                    // backwards-compatible ports
                    ["FOO_SERVICE_HOST"] = "127.0.0.1",
                    ["FOO_SERVICE_PORT"] = "5050",
                    ["FOO_PORT"] = "tcp://127.0.0.1:5050",
                    // named ports
                    ["FOO_PORT_5050_TCP_PROTO"] = "tcp",
                    ["FOO_PORT_5050_TCP"] = "tcp://127.0.0.1:5050",
                    ["FOO_PORT_5050_TCP_PORT"] = "5050",
                    ["FOO_PORT_5050_TCP_ADDR"] = "127.0.0.1",
                }
            };

            // single endpoint with multiple named ports
            yield return new object[]
            {
                new[]
                {
                    new EndpointInfo
                    {
                        DnsName = "foo",
                        LocalIP = System.Net.IPAddress.Parse("127.0.0.1"),
                        Ports = new[]
                        {
                            new PortPair(5050, 80, "tcp", "http"),
                            new PortPair(5051, 443, "tcp", "tls")
                        }
                    }
                },
                new Dictionary<string, string>
                {
                    // backwards-compatible ports
                    ["FOO_SERVICE_HOST"] = "127.0.0.1",
                    ["FOO_SERVICE_PORT"] = "5050",
                    ["FOO_PORT"] = "tcp://127.0.0.1:5050",
                    // named ports for first port pair
                    ["FOO_PORT_5050_TCP_PROTO"] = "tcp",
                    ["FOO_PORT_5050_TCP"] = "tcp://127.0.0.1:5050",
                    ["FOO_PORT_5050_TCP_PORT"] = "5050",
                    ["FOO_PORT_5050_TCP_ADDR"] = "127.0.0.1",
                    ["FOO_SERVICE_PORT_HTTP"] = "5050",
                    // named ports for second port pair
                    ["FOO_PORT_5051_TCP_PROTO"] = "tcp",
                    ["FOO_PORT_5051_TCP"] = "tcp://127.0.0.1:5051",
                    ["FOO_PORT_5051_TCP_PORT"] = "5051",
                    ["FOO_PORT_5051_TCP_ADDR"] = "127.0.0.1",
                    ["FOO_SERVICE_PORT_TLS"] = "5051",
                }
            };

            // multiple endpoints with simple and named ports
            yield return new object[]
            {
                new[]
                {
                    new EndpointInfo
                    {
                        DnsName = "foo",
                        LocalIP = System.Net.IPAddress.Parse("127.0.0.1"),
                        Ports = new[]
                        {
                            new PortPair(5050, 80, "tcp", "http"),
                            new PortPair(5051, 443, "tcp", "tls")
                        }
                    },
                    new EndpointInfo
                    {
                        DnsName = "bar",
                        LocalIP = System.Net.IPAddress.Parse("127.0.0.2"),
                        Ports = new[] { new PortPair(5050, 80) }
                    }
                },
                new Dictionary<string, string>
                {
                    // first endpoints backwards-compatible ports
                    ["FOO_SERVICE_HOST"] = "127.0.0.1",
                    ["FOO_SERVICE_PORT"] = "5050",
                    ["FOO_PORT"] = "tcp://127.0.0.1:5050",
                    // first endpoints named ports for first port pair
                    ["FOO_PORT_5050_TCP_PROTO"] = "tcp",
                    ["FOO_PORT_5050_TCP"] = "tcp://127.0.0.1:5050",
                    ["FOO_PORT_5050_TCP_PORT"] = "5050",
                    ["FOO_PORT_5050_TCP_ADDR"] = "127.0.0.1",
                    ["FOO_SERVICE_PORT_HTTP"] = "5050",
                    // first endpoints named ports for second port pair
                    ["FOO_PORT_5051_TCP_PROTO"] = "tcp",
                    ["FOO_PORT_5051_TCP"] = "tcp://127.0.0.1:5051",
                    ["FOO_PORT_5051_TCP_PORT"] = "5051",
                    ["FOO_PORT_5051_TCP_ADDR"] = "127.0.0.1",
                    ["FOO_SERVICE_PORT_TLS"] = "5051",
                    // second endpoints backwards-compatible ports
                    ["BAR_SERVICE_HOST"] = "127.0.0.2",
                    ["BAR_SERVICE_PORT"] = "5050",
                    ["BAR_PORT"] = "tcp://127.0.0.2:5050",
                    // second endpoints named ports for first port pair
                    ["BAR_PORT_5050_TCP_PROTO"] = "tcp",
                    ["BAR_PORT_5050_TCP"] = "tcp://127.0.0.2:5050",
                    ["BAR_PORT_5050_TCP_PORT"] = "5050",
                    ["BAR_PORT_5050_TCP_ADDR"] = "127.0.0.2",
                }
            };

            // managed identity
            yield return new object[]
            {
                new[]
                {
                    new EndpointInfo
                    {
                        DnsName = ManagedIdentity.TargetServiceNameOnLocalMachine,
                        LocalIP = System.Net.IPAddress.Parse("127.0.0.1"),
                        Ports = new[] { new PortPair(5050, 80, "tcp") }
                    }
                },
                new Dictionary<string, string>
                {
                    // backwards-compatible ports
                    ["MANAGEDIDENTITYFORBRIDGETOKUBERNETES_SERVICE_HOST"] = "127.0.0.1",
                    ["MANAGEDIDENTITYFORBRIDGETOKUBERNETES_SERVICE_PORT"] = "5050",
                    ["MANAGEDIDENTITYFORBRIDGETOKUBERNETES_PORT"] = "tcp://127.0.0.1:5050",
                    // named ports
                    ["MANAGEDIDENTITYFORBRIDGETOKUBERNETES_PORT_5050_TCP_PROTO"] = "tcp",
                    ["MANAGEDIDENTITYFORBRIDGETOKUBERNETES_PORT_5050_TCP"] = "tcp://127.0.0.1:5050",
                    ["MANAGEDIDENTITYFORBRIDGETOKUBERNETES_PORT_5050_TCP_PORT"] = "5050",
                    ["MANAGEDIDENTITYFORBRIDGETOKUBERNETES_PORT_5050_TCP_ADDR"] = "127.0.0.1",
                    // specific use case for managed identity
                    [ManagedIdentity.MSI_ENDPOINT_EnvironmentVariable] = "http://127.0.0.1:5050/metadata/identity/oauth2/token",
                }
            };

            // single basic endpoint in another ns
            yield return new object[]
            {
                new[] {
                    new EndpointInfo
                    {
                        DnsName = "foo.k8sns", // when !isInWorkloadNamespace
                        LocalIP = System.Net.IPAddress.Parse("127.0.0.1"),
                        Ports = new[] { new PortPair(5050, 80) }
                    }
                },
                new Dictionary<string, string>
                {
                    // backwards-compatible ports
                    ["FOO_K8SNS_SERVICE_HOST"] = "127.0.0.1",
                    ["FOO_K8SNS_SERVICE_PORT"] = "5050",
                    ["FOO_K8SNS_PORT"] = "tcp://127.0.0.1:5050",
                    // named ports
                    ["FOO_K8SNS_PORT_5050_TCP_PROTO"] = "tcp",
                    ["FOO_K8SNS_PORT_5050_TCP"] = "tcp://127.0.0.1:5050",
                    ["FOO_K8SNS_PORT_5050_TCP_PORT"] = "5050",
                    ["FOO_K8SNS_PORT_5050_TCP_ADDR"] = "127.0.0.1",
                }
            };

            // single endpoint with multiple named ports in another ns
            yield return new object[]
            {
                new[]
                {
                    new EndpointInfo
                    {
                        DnsName = "foo.k8sns", // when !isInWorkloadNamespace
                        LocalIP = System.Net.IPAddress.Parse("127.0.0.1"),
                        Ports = new[]
                        {
                            new PortPair(5050, 80, "tcp", "http"),
                            new PortPair(5051, 443, "tcp", "tls")
                        }
                    }
                },
                new Dictionary<string, string>
                {
                    // backwards-compatible ports
                    ["FOO_K8SNS_SERVICE_HOST"] = "127.0.0.1",
                    ["FOO_K8SNS_SERVICE_PORT"] = "5050",
                    ["FOO_K8SNS_PORT"] = "tcp://127.0.0.1:5050",
                    // named ports for first port pair
                    ["FOO_K8SNS_PORT_5050_TCP_PROTO"] = "tcp",
                    ["FOO_K8SNS_PORT_5050_TCP"] = "tcp://127.0.0.1:5050",
                    ["FOO_K8SNS_PORT_5050_TCP_PORT"] = "5050",
                    ["FOO_K8SNS_PORT_5050_TCP_ADDR"] = "127.0.0.1",
                    ["FOO_K8SNS_SERVICE_PORT_HTTP"] = "5050",
                    // named ports for second port pair
                    ["FOO_K8SNS_PORT_5051_TCP_PROTO"] = "tcp",
                    ["FOO_K8SNS_PORT_5051_TCP"] = "tcp://127.0.0.1:5051",
                    ["FOO_K8SNS_PORT_5051_TCP_PORT"] = "5051",
                    ["FOO_K8SNS_PORT_5051_TCP_ADDR"] = "127.0.0.1",
                    ["FOO_K8SNS_SERVICE_PORT_TLS"] = "5051",
                }
            };

            // endpoints with multiple named ports for headless services
            yield return new object[]
            {
                new[]
                {
                    new EndpointInfo
                    {
                        DnsName = "podname.servicename", // for headless it is combination of hostname.service from get endpoints
                        LocalIP = System.Net.IPAddress.Parse("127.0.0.1"),
                        Ports = new[]
                        {
                            new PortPair(localPort: 5050, remotePort: 80, name: "http"),
                            new PortPair(localPort: 5051, remotePort:443, name: "client", protocol: "client")
                        }
                    }
                },
                new Dictionary<string, string>
                {
                    // backwards-compatible ports
                    ["PODNAME_SERVICENAME_SERVICE_HOST"] = "127.0.0.1",
                    ["PODNAME_SERVICENAME_SERVICE_PORT"] = "5050",
                    ["PODNAME_SERVICENAME_PORT"] = "tcp://127.0.0.1:5050",
                    // named ports for first port pair
                    ["PODNAME_SERVICENAME_PORT_5050_TCP_PROTO"] = "tcp",
                    ["PODNAME_SERVICENAME_PORT_5050_TCP"] = "tcp://127.0.0.1:5050",
                    ["PODNAME_SERVICENAME_PORT_5050_TCP_PORT"] = "5050",
                    ["PODNAME_SERVICENAME_PORT_5050_TCP_ADDR"] = "127.0.0.1",
                    ["PODNAME_SERVICENAME_SERVICE_PORT_HTTP"] = "5050",
                    // named ports for SECOND port pair
                    ["PODNAME_SERVICENAME_PORT_5051_CLIENT_PROTO"] = "client",
                    ["PODNAME_SERVICENAME_PORT_5051_CLIENT"] = "client://127.0.0.1:5051",
                    ["PODNAME_SERVICENAME_PORT_5051_CLIENT_PORT"] = "5051",
                    ["PODNAME_SERVICENAME_PORT_5051_CLIENT_ADDR"] = "127.0.0.1",
                    ["PODNAME_SERVICENAME_SERVICE_PORT_CLIENT"] = "5051",
                    
                }
            };
        }

        [Theory]
        [MemberData(nameof(TestData))]
        public void CreateEnvVariablesForK8s_Returns_ExactlyTheExpectedResults(IEnumerable<EndpointInfo> endpoints, Dictionary<string, string> expected)
        {
            var workloadInfo = new WorkloadInfo
            {
                ReachableEndpoints = new List<EndpointInfo>(endpoints),
                EnvironmentVariables = new Dictionary<string, string>()
            };

            var result = _localEnvironmentManager.CreateEnvVariablesForK8s(workloadInfo);

            Assert.True(result.All(e => expected.Contains(e)));
        }
    }
}
