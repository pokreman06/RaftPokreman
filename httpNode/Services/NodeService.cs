using System.Collections.Concurrent;
using raftLibrary;
using httpNode.Classes;

namespace httpNode.Services
{
    public class NodeService
    {
        private readonly ConcurrentDictionary<string, INode> _nodes = new ConcurrentDictionary<string, INode>();
        private Node? _currentNode;

        public NodeService()
        {
            _currentNode = new Node(isRandomized: true);
            
            // Check for NODE_NAME environment variable
            var nodeName = Environment.GetEnvironmentVariable("NODE_NAME");
            if (!string.IsNullOrEmpty(nodeName))
            {
                _currentNode.Name = nodeName;
            }
            
            _nodes.TryAdd(_currentNode.Name, _currentNode);
            
            // Initialize peers from environment variable if provided
            var peersCSV = Environment.GetEnvironmentVariable("NODE_PEERS_CSV");
            if (!string.IsNullOrEmpty(peersCSV))
            {
                InitializePeersFromCSV(peersCSV);
            }
        }

        /// <summary>
        /// Initializes peers from a CSV string in format "Name1:Endpoint1,Name2:Endpoint2"
        /// </summary>
        public void InitializePeersFromCSV(string csv)
        {
            if (string.IsNullOrEmpty(csv) || _currentNode == null)
            {
                return;
            }

            var peerEntries = csv.Split(',', StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var entry in peerEntries)
            {
                var trimmedEntry = entry.Trim();
                var colonIndex = trimmedEntry.IndexOf(':');
                
                if (colonIndex > 0 && colonIndex < trimmedEntry.Length - 1)
                {
                    var name = trimmedEntry.Substring(0, colonIndex).Trim();
                    var endpoint = trimmedEntry.Substring(colonIndex + 1).Trim();
                    
                    var proxyNode = new HttpProxyNode(endpoint, name);
                    _currentNode.Peers.Add(proxyNode);
                    _nodes.TryAdd(name, proxyNode);
                    Console.WriteLine($"[NodeService] Added peer: {name} at {endpoint}");
                }
                else
                {
                    Console.WriteLine($"[NodeService] Invalid peer entry format: {trimmedEntry}. Expected 'Name:Endpoint'");
                }
            }
        }

        /// <summary>
        /// Gets an INode by name, returns the current node if not found.
        /// </summary>
        public INode GetNodeByName(string name)
        {
            if (_nodes.TryGetValue(name, out var node))
            {
                return node;
            }
            return _currentNode ?? throw new InvalidOperationException("Current node is not initialized");
        }

        /// <summary>
        /// Gets or creates the current node instance.
        /// </summary>
        public Node GetCurrentNode()
        {
            return _currentNode ?? throw new InvalidOperationException("Current node is not initialized");
        }

        /// <summary>
        /// Sets the current active node.
        /// </summary>
        public void SetCurrentNode(string nodeId)
        {
            if (_nodes.TryGetValue(nodeId, out var node) && node is Node localNode)
            {
                _currentNode = localNode;
            }
            else
            {
                throw new KeyNotFoundException($"Node with ID {nodeId} not found or is not a local node");
            }
        }

        /// <summary>
        /// Adds a new node to the service.
        /// </summary>
        public INode CreateNode(bool isRandomized = true)
        {
            var node = new Node(isRandomized: isRandomized);
            _nodes.TryAdd(node.Name, node);
            return node;
        }

        /// <summary>
        /// Gets a node by its ID/Name.
        /// </summary>
        public INode? GetNode(string nodeId)
        {
            _nodes.TryGetValue(nodeId, out var node);
            return node;
        }

        /// <summary>
        /// Gets all managed nodes.
        /// </summary>
        public IEnumerable<INode> GetAllNodes()
        {
            return _nodes.Values;
        }

        /// <summary>
        /// Removes a node by its ID/Name.
        /// </summary>
        public bool RemoveNode(string nodeId)
        {
            var removed = _nodes.TryRemove(nodeId, out _);
            if (_currentNode?.Name == nodeId)
            {
                _currentNode = _nodes.Values.OfType<Node>().FirstOrDefault();
            }
            return removed;
        }
    }
}
