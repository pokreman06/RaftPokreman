using System.Text.Json;
using raftLibrary;

namespace httpNode.Classes
{
    public class HttpProxyNode : INode
    {
        private readonly HttpClient _httpClient;
        private readonly string _endpoint;

        public string Name { get; set; }

        public HttpProxyNode(string endpoint, string name = "")
        {
            _endpoint = endpoint.TrimEnd('/');
            _httpClient = new HttpClient();
            Name = name ?? Guid.NewGuid().ToString();
        }

        private async Task<T?> SendRequest<T>(string method, string path, object? data = null)
        {
            try
            {
                var url = $"{_endpoint}{path}";
                HttpResponseMessage response;

                switch (method.ToUpper())
                {
                    case "POST":
                        var content = new StringContent(JsonSerializer.Serialize(data), System.Text.Encoding.UTF8, "application/json");
                        response = await _httpClient.PostAsync(url, content);
                        break;
                    case "GET":
                        response = await _httpClient.GetAsync(url);
                        break;
                    default:
                        throw new ArgumentException($"Unsupported HTTP method: {method}");
                }

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<T>(jsonContent);
                }
                return default;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[HttpProxyNode] Error sending request to {_endpoint}: {ex.Message}");
                return default;
            }
        }

        public async Task<string> RecieveCommand(string logEntry)
        {
            var result = await SendRequest<string>("POST", "/api/node/command", new { command = logEntry });
            return result ?? "";
        }

        public async Task<bool> VoteFor(int term, INode voter)
        {
            var result = await SendRequest<bool>("POST", "/api/node/votefor", new { term, voterId = voter.Name });
            return result;
        }

        public async Task<bool> AppendEntriesResponse(int term, INode follower, int lastLogIndex, bool success)
        {
            var result = await SendRequest<bool>("POST", "/api/node/appendentriesresponse", 
                new { term, followerId = follower.Name, lastLogIndex, success });
            return result;
        }

        public async Task<bool> AppendEntries(int term, INode leaderId, int prevLogIndex, int prevLogTerm, LogEntry[] entries, int leaderCommit)
        {
            var result = await SendRequest<bool>("POST", "/api/node/appendentries", 
                new { term, leaderId = leaderId.Name, prevLogIndex, prevLogTerm, entries, leaderCommit });
            return result;
        }

        public async Task<bool> RequestVote(int term, INode candidateId, int lastLogIndex, int lastLogTerm)
        {
            var result = await SendRequest<bool>("POST", "/api/node/requestvote", 
                new { term, candidateId = candidateId.Name, lastLogIndex, lastLogTerm });
            return result;
        }
    }
}
