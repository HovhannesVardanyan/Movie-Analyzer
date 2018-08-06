using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Newtonsoft.Json;
using System.IO;
namespace MovieAnalyzer
{
    /// <summary>
    /// 
    /// This is the implementation of Graph Data Structure, which is used for Movie Analyzer
    /// 
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    class Graph<TKey,TValue,TEdge> : IEnumerable<TValue>, ICloneable
    {
        public const int NumberOfCores = 6;
        private static int clusters = 0;
        private static object obj = new object();
        private Random random = new Random();

        [JsonProperty]
        private Dictionary<TKey, Node<TKey, TValue, TEdge>> nodes = new Dictionary<TKey, Node<TKey, TValue, TEdge>>();

        public int Count => nodes.Count;
        private int maxCluster = -1;
        public TValue this[TKey key] {
            get {
                return nodes[key].Data;
            }
        }
        public void AddNode(TKey key,TValue value)
        {
            Node<TKey, TValue, TEdge> node = new Node<TKey, TValue, TEdge>(key, value, null);
            nodes.Add(key, node);
        }
        public void ConnectNodes(TKey t1, TKey t2,TEdge edge)
        {
            nodes[t1].ConnectByDirectionally(nodes[t2], edge);
        }
        public void BFS(TKey source)
        {
            CleanUp();
            Queue<Node<TKey, TValue, TEdge>> queue = new Queue<Node<TKey, TValue, TEdge>>();
            Node<TKey, TValue, TEdge> node = nodes[source];

            queue.Enqueue(node);
            node.Seen = true;

            while (queue.Count != 0)
            {
                Node<TKey, TValue, TEdge> temp = queue.Dequeue();

                Console.WriteLine(temp.Data);

                foreach (var item in temp.Connections)
                {
                    if (!item.EndPoint.Seen)
                    {
                        item.EndPoint.Seen = true;
                        queue.Enqueue(item.EndPoint);
                    }
                }
            }

            CleanUp();
        }
        private void CleanUp()
        {
            clusters = 0;
            foreach (var item in nodes)
            {
                item.Value.Seen = false;
                item.Value.Distance = -1;
                item.Value.Previous = new LinkedList<TKey>();
                item.Value.Cluster = -1;
            }
        }
        public void DFS(TKey source)
        {
            Stack<Node<TKey, TValue, TEdge>> stack = new Stack<Node<TKey, TValue, TEdge>>();
            stack.Push(nodes[source]);
            nodes[source].Seen = true;

            while (!stack.IsEmpty)
            {
                Node<TKey, TValue, TEdge> temp = stack.Pop();

                Console.WriteLine(temp.Data);

                foreach (var item in temp.Connections)
                {
                    if (!item.EndPoint.Seen)
                    {
                        item.EndPoint.Seen = true;
                        stack.Push(item.EndPoint);
                    }
                }

            }

        }
        public int MinimumSpanTree()
        {
            int weight = 0;
            if (nodes.Count <= 1)
                return 0;

            CleanUp();
            Node<TKey, TValue, TEdge> beg = nodes.ElementAt(0).Value;
            LinkedList<Node<TKey, TValue, TEdge>> list = new LinkedList<Node<TKey, TValue, TEdge>>();
            list.AddLast(beg);
            Node<TKey, TValue, TEdge> temp = null;
            while (list.Count != nodes.Count)
            {
                int min = int.MaxValue;
                foreach (var item in list)
                {
                    Console.WriteLine(item.Key + "  ");
                    foreach (var connection in item.Connections)
                    {
                        if (connection.Data is int length)
                        {
                            if (min > length && !list.Contains(connection.EndPoint))
                            {
                                min = length;
                                temp = connection.EndPoint;
                            }
                        }
                        else
                            throw new Exception("Invalid data on the edges");
                    }
                }
                weight += min;
                list.AddLast(temp);
            }
            Console.WriteLine();
            foreach (var item in list)
            {
                Console.Write(item.Data + " ");
            }
            Console.WriteLine();
            return weight;
        }
        public void ChangeValue(TKey key,TValue value)
        {
            nodes[key].Data = value;
        }
        public void RemoveNode(TKey item)
        {
            Node<TKey, TValue, TEdge> node = nodes[item];
            foreach (var i in node.Connections)
                i.EndPoint.RemoveConnection(item);
            nodes.Remove(item);
        }
        private void DisconnectNodes(TKey t1, TKey t2,bool refresh = false)
        {
            var n1 = nodes[t1];
            var n2 = nodes[t2];
            n1.RemoveConnection(n2.Key);
            n2.RemoveConnection(n1.Key);
            if (refresh)
                this.Refresh();
        }
        public void DisconnectNodes(TKey t1, TKey t2)
        {
            DisconnectNodes(t1, t2, true);
        }
        public List<(TKey, TKey)> MinCut()
        {
            List<(TKey, TKey)> lst = null;
            long count = (long)(Math.Log(nodes.Count) * nodes.Count * nodes.Count);
            for (long i = 1; i <= count; i++)
            {
                var temp = ((Graph<TKey,TValue,TEdge>)(this.Clone())).MinCutU();
                //Console.WriteLine(temp.Count);
                if (lst == null || lst.Count > temp.Count)
                {
                    lst = temp;
                }
            }
            return lst;
        }
        private List<(TKey,TKey)> MinCutU()
        {
            int n = nodes.Count;
            while (n > 2)
            {
                //this.Print();
                int index = random.Next(0, nodes.Count);

                while(nodes.ElementAt(index).Value.Connections.Count == 0)
                    index = random.Next(0, nodes.Count);

                TKey key1 = nodes.ElementAt(index).Key;

                int ind = random.Next(0, nodes[key1].Connections.Count);

                var key2 = nodes[key1].Connections[ind].EndPoint.Key;

                MergeNodes(key1,key2);
                n--;
            }
            LinkedList<(TKey, TKey)> lst = new LinkedList<(TKey, TKey)>();

            foreach (var item in nodes)
            {
                foreach (var connection in item.Value.Connections)
                {
                    lst.AddLast((item.Key, connection.EndPoint.Key));
                }
            }
            
            return lst.ToList();
        }
        public List<Edge<TKey,TValue,TEdge>> Connections(TKey s, TKey d)
        {
            List<Edge<TKey, TValue, TEdge>> t = new List<Edge<TKey, TValue, TEdge>>();
            foreach (var con in nodes[s].Connections)
            {
                if (con.EndPoint.Key.Equals(d))
                    t.Add(con);
            }
            return t;
        }
        public bool ExistsConnection(TKey s, TKey d, TEdge data)
        {
            foreach (var con in nodes[s].Connections)
            {
                if (con.Data.Equals(data) && con.EndPoint.Key.Equals(d))
                    return true;
            }
            return false;
        }
        public object Clone()
        {
            Graph<TKey, TValue, TEdge> gr = new Graph<TKey, TValue, TEdge>();
            foreach (var item in this.nodes)
            {
                gr.AddNode(item.Key, item.Value.Data);
            }
            foreach (var item in this.nodes)
            {
                foreach (var elem in this.nodes)
                {
                    var edges = this.Connections(item.Key, elem.Key);

                    foreach (var edge in edges)
                    {
                        if (!gr.ExistsConnection(item.Key, elem.Key, edge.Data) || !gr.ExistsConnection(elem.Key, item.Key, edge.Data))
                        {
                            gr.ConnectNodes(item.Key, elem.Key, edge.Data);
                        }
                    }
                    
                }
            }
            return gr;
        }
        public bool ExistsNode(TKey item)
        {
            return nodes.ContainsKey(item);
        }
        private void MergeNodes(TKey t1, TKey t2)
        {
            var node1 = nodes[t1];
            var node2 = nodes[t2];
            if (node1.Cluster == -1 && node2.Cluster == -1)
            { 
                node1.Cluster = ++maxCluster;
                node2.Cluster = maxCluster;
                this.DisconnectNodes(t1,t2,false);
                
                node1.Refresh();
                node2.Refresh();
            }
            else if (node1.Cluster == -1 || node2.Cluster == -1)
            {
                if (node2.Cluster == -1)
                {
                    var temp = node1;
                    node1 = node2;
                    node2 = temp;
                }
                
                node1.Cluster = node2.Cluster;
                DisconnectFromSuperNode(node1.Key, node2.Cluster);
            }
            else
            {
                int c1 = node1.Cluster, c2 = node2.Cluster;
                foreach (var item in nodes)
                {
                    if (item.Value.Cluster == c2)
                    {
                        item.Value.Cluster = c1;
                        this.DisconnectFromSuperNode(item.Value.Key,c1);
                    }
                }
                
            }


        }
        private void DisconnectFromSuperNode(TKey t1, int id)
        {
            foreach (var item in nodes[t1].Connections)
            {
                if (item.EndPoint.Cluster == id)
                {
                    this.DisconnectNodes(item.EndPoint.Key, t1);
                }
            }
            Refresh();
        }
        private void Refresh()
        {
            foreach (var item in nodes)
            {
                item.Value.Refresh();
            }
        }
        private Graph<TKey, TValue, TEdge> SubGraph(TKey source,bool cleanUp = true)
        {
            Graph<TKey, TValue, TEdge> graph = new Graph<TKey, TValue, TEdge>();
            if (graph.Count == 1)
                return graph;
            if(cleanUp)
                CleanUp();
            Queue<Node<TKey, TValue, TEdge>> queue = new Queue<Node<TKey, TValue, TEdge>>();
            Node<TKey, TValue, TEdge> node = nodes[source];

            queue.Enqueue(node);
            node.Seen = true;

            while (queue.Count != 0)
            {
                Node<TKey, TValue, TEdge> temp = queue.Dequeue();
                graph.AddNode(temp.Key, temp.Data);
                foreach (var item in temp.Connections)
                {
                    if (graph.ExistsNode(item.EndPoint.Key) && !graph.ExistsConnection(temp.Key, item.EndPoint.Key,item.Data))
                    {
                        graph.ConnectNodes(temp.Key, item.EndPoint.Key,item.Data);
                    }
                    if (!item.EndPoint.Seen)
                    {
                        item.EndPoint.Seen = true;
                        queue.Enqueue(item.EndPoint);
                    }
                }
            }
            if(cleanUp)
                CleanUp();
            return graph;
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            foreach (var item in nodes)
            {
                sb.Append(item.Value + "  ");
            }
            return sb.ToString();
        }
        

    
        //probably the most effective one
        public static List<Graph<TKey, TValue, TEdge>> SubGroupsParallel(Graph<TKey, TValue, TEdge> graph, int maxSize,string directory)
        {
            clusters = 0;

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            else
            {
                var files = Directory.GetFiles(directory,"*.json");
                foreach (var file in files)
                {
                    string[] p = file.Split('.')[0].Split('\\');

                    int num = int.Parse(p[p.Length-1]);

                    if (clusters < num)
                        clusters = num;
                }
            }

            List<Graph<TKey, TValue, TEdge>> list = GetConnectedSubgroupsIsolated(graph);

            List<Graph<TKey, TValue, TEdge>> smalls = new List<Graph<TKey, TValue, TEdge>>();

            List<Graph<TKey, TValue, TEdge>> bigs = new List<Graph<TKey, TValue, TEdge>>();
            List<Graph<TKey, TValue, TEdge>> ans = new List<Graph<TKey, TValue, TEdge>>();

            foreach (var item in list)
            {
                if (item.Count <= maxSize * 2)
                    smalls.Add(item);
                else
                    bigs.Add(item);
            }

            ans.AddRange(SubGroupsParallel(smalls, maxSize, directory, false));

            ans.AddRange(SubGroupsParallel(bigs, maxSize, directory, true, NumberOfCores / 3));

            return ans;
            
        }

        //helpers
        private static List<Graph<TKey, TValue, TEdge>> SubGroupsParallel(List<Graph<TKey, TValue, TEdge>> link1, int maxSize,string directory,bool para = false,int n = NumberOfCores)
        {
            int TaskCount = link1.Count / n + 1;
            Task<List<Graph<TKey, TValue, TEdge>>>[] tasks = new Task<List<Graph<TKey, TValue, TEdge>>>[n];
            LinkedList<Graph<TKey, TValue, TEdge>>[] register = new LinkedList<Graph<TKey, TValue, TEdge>>[n];
            for (int j = 0; j < n; j++)
                register[j] = new LinkedList<Graph<TKey, TValue, TEdge>>();

            for (int i = 0; i < link1.Count; i++)
                register[i % n].AddLast(link1[i]);
            

            for (int i = 0; i < n; i++)
            {
                var linkedList = register[i];
                tasks[i] = System.Threading.Tasks.Task.Run(() => {
                    var ans = new List<Graph<TKey, TValue, TEdge>>();
                    if (!para)
                        foreach (var item in linkedList)
                        {
                            var local = GetConnectedSubgroups(item, maxSize);
                            ans.AddRange(local);
                            //Print(local);
                            lock(obj)
                            {
                                Interlocked.Increment(ref clusters);
                                SaveAsJson(local, directory + @"\" + clusters);
                            }
                        }
                    else
                        foreach (var item in linkedList)
                        {
                            var local = SubGroupDivide(item, maxSize,directory);
                            ans.AddRange(local);
                            //Print(local);
                            lock (obj)
                            {
                                Interlocked.Increment(ref clusters);
                                SaveAsJson(local,directory + @"\" + clusters);
                            }
                        }
                    
                    return ans;
                });
            }
            var a = new List<Graph<TKey, TValue, TEdge>>();
            foreach (var task in tasks)
                a.AddRange(task.Result);
            return a;
        }
        private static List<Graph<TKey, TValue, TEdge>> SubGroupDivide(Graph<TKey, TValue, TEdge> graph, int maxSize,string directory)
        {
            List<Graph<TKey, TValue, TEdge>> ans = new List<Graph<TKey, TValue, TEdge>>();
            Queue<Task<List<Graph<TKey, TValue, TEdge>>>> queue = new Queue<Task<List<Graph<TKey, TValue, TEdge>>>>();
            
            queue.Enqueue(System.Threading.Tasks.Task.Run(() =>
            {
                return Split(graph, maxSize);
            }));
               
            while (queue.Count != 0)
            {
                var temp = queue.Dequeue();
                var d = temp.Result;

                if (d.Count == 1)
                {
                    //Console.WriteLine("\n"+d[0]);

                    lock (obj)
                    {
                        Interlocked.Increment(ref clusters);
                        d[0].SaveAsJson(directory + @"\" + clusters);
                    }

                    ans.Add(d[0]);
                }
                else
                {
                    
                    var small = d[0].Count < d[1].Count ? d[0] : d[1];
                    if (small.Count <= maxSize)
                    {
                        //Console.WriteLine("\n" + small);
                        lock (obj)
                        {
                            Interlocked.Increment(ref clusters);
                            small.SaveAsJson(directory + @"\" + clusters);
                        }

                        ans.Add(small);
                    }
                    else
                        queue.Enqueue(System.Threading.Tasks.Task.Run(() =>
                        {
                            return Split(small, maxSize);
                        }));

                    var large = d[0].Count < d[1].Count ? d[1] : d[0];

                    if (large.Count <= maxSize)
                    {
                        //Console.WriteLine("\n" + large);
                        lock (obj)
                        {
                            Interlocked.Increment(ref clusters);
                            large.SaveAsJson(directory + @"\" + clusters);
                        }

                        ans.Add(large);
                    }
                    else
                        queue.Enqueue(System.Threading.Tasks.Task.Run(() =>
                        {
                            return Split(large, maxSize);
                        }));
                }
            }

            return ans;
        }

        private static void SaveAsJson(List<Graph<TKey,TValue,TEdge>> list, string path)
        {
                foreach (var item in list)
                    item.SaveAsJson(path);
        }
        private void SaveAsJson(string path)
        {
            using (FileStream fs = new FileStream(path + ".json", FileMode.Create))
            using (StreamWriter sw = new StreamWriter(fs))
            {
                sw.WriteLine(JsonConvert.SerializeObject(this));
            }
        }

        private static void Print(List<Graph<TKey,TValue,TEdge>> lst)
        {
            foreach (var graph in lst)
            {
                Console.WriteLine($"\n {graph} ");
            }
        }
        public static List<Graph<TKey, TValue, TEdge>> SubGroups(Graph<TKey, TValue, TEdge> graph, int maxSize)
        {
            List<Graph<TKey, TValue, TEdge>> graphs = GetConnectedSubgroupsIsolated(graph);

            List<Graph<TKey, TValue, TEdge>> ans = new List<Graph<TKey, TValue, TEdge>>();

            foreach (var g in graphs)
            {
                ans.AddRange(GetConnectedSubgroups(g,maxSize));
            }

            return ans;
        }
        private static List<Graph<TKey, TValue, TEdge>> Split(Graph<TKey, TValue, TEdge> graph, int MaxSize)
        {
            if (graph.Count <= MaxSize)
                return new List<Graph<TKey, TValue, TEdge>>() { graph };
            var crossEdges = graph.MinCut();
            TKey left = crossEdges[0].Item1, right = crossEdges[0].Item2;
            foreach (var tuple in crossEdges)
            {
                graph.DisconnectNodes(tuple.Item1, tuple.Item2);
            }

            var g1 = graph.SubGraph(left);
            var g2 = graph.SubGraph(right);
            return new List<Graph<TKey, TValue, TEdge>>() { g1, g2};
        }
        private static List<Graph<TKey, TValue, TEdge>> GetConnectedSubgroupsIsolated(Graph<TKey, TValue, TEdge> graph)
        {
            graph.CleanUp();
            Node<TKey, TValue, TEdge> node = null;
            List<Graph<TKey, TValue, TEdge>> lst = new List<Graph<TKey, TValue, TEdge>>();
            while ((node = AllCovered(graph)) != null)
            {
                lst.Add(graph.SubGraph(node.Key, false));
            }

            graph.CleanUp();
            return lst;
        }
        private static Node<TKey,TValue,TEdge> AllCovered(Graph<TKey,TValue,TEdge> g)
        {
            foreach (var node in g.nodes)
                if (!node.Value.Seen)
                    return node.Value;
            return null;
        }
        private static List<Graph<TKey, TValue, TEdge>> GetConnectedSubgroups(Graph<TKey,TValue,TEdge> graph,int maxSize)
        {
            if (graph.Count <= maxSize)
                return new List<Graph<TKey, TValue, TEdge>>() { graph };
            var crossEdges = graph.MinCut();
            TKey left = crossEdges[0].Item1, right = crossEdges[0].Item2;
            foreach (var tuple in crossEdges)
            {
                graph.DisconnectNodes(tuple.Item1,tuple.Item2);
            }

            var g1 = graph.SubGraph(left);
            var g2 = graph.SubGraph(right);

            List<Graph<TKey, TValue, TEdge>> list = new List<Graph<TKey, TValue, TEdge>>();
            list.AddRange(GetConnectedSubgroups(g1, maxSize));
            list.AddRange(GetConnectedSubgroups(g2, maxSize));
            
            return list;
        }
        public void Print()
        {
            Console.WriteLine();
            foreach(var item in nodes)
                Console.Write(item.Key + " ");
            Console.WriteLine();
        }
        public int ShortestPath(TKey source, TKey destination)
        {
            CleanUp();
            var src = nodes[source];
            List<Node<TKey,TValue,TEdge>> lst = new List<Node<TKey, TValue, TEdge>>();
            lst.Add(src);
            lst[0].Distance = 0;
            while (lst.Count != 0)
            {
                var temp = lst[0];
                temp.Seen = true;
                lst.RemoveAt(0);

                foreach (var edge in temp.Connections)
                {
                    if (edge.Data is int i)
                    {
                        if (i + temp.Distance < edge.EndPoint.Distance || edge.EndPoint.Distance == -1)
                            edge.EndPoint.Distance = i + temp.Distance;
                    }
                    if (!edge.EndPoint.Seen && !lst.Contains(edge.EndPoint))
                        lst.Add(edge.EndPoint);
                }
                lst.Sort(new Comp<TKey,TValue,TEdge>());
            }
            return nodes[destination].Distance;
        }
        public int ShortestPathHeaps(TKey source, TKey destination)
        {

            CleanUp();
            var src = nodes[source];
            Heap<Node<TKey,TValue,TEdge>> heap = new Heap<Node<TKey, TValue, TEdge>>((a,b) => a.Distance < b.Distance);
            src.Distance = 0;
            heap.Insert(src);
            while (!heap.isEmpty)
            {
                var temp = heap.Extract();
                temp.Seen = true;
                foreach (var edge in temp.Connections)
                {
                    if (edge.Data is int i)
                    {
                        if (!edge.EndPoint.Seen)
                        {
                            if (edge.EndPoint.Distance == -1)
                            {
                                edge.EndPoint.Distance = i + temp.Distance;
                                heap.Insert(edge.EndPoint);
                            }

                            else if (i + temp.Distance < edge.EndPoint.Distance)
                            {
                                heap.Delete(edge.EndPoint.ID);
                                edge.EndPoint.Distance = i + temp.Distance;
                                heap.Insert(edge.EndPoint);
                            }
                        }
                    }
                    else
                        return -1;
                }
            }
            return nodes[destination].Distance;
        }
        IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
        {
            CleanUp();
            Queue<Node<TKey, TValue, TEdge>> queue = new Queue<Node<TKey, TValue, TEdge>>();

            Node<TKey, TValue, TEdge> node = nodes.ElementAt(0).Value;

            queue.Enqueue(node);
            node.Seen = true;

            while (queue.Count != 0)
            {
                Node<TKey, TValue, TEdge> temp = queue.Dequeue();

                yield return temp.Data;

                foreach (var item in temp.Connections)
                {
                    if (!item.EndPoint.Seen)
                    {
                        item.EndPoint.Seen = true;
                        queue.Enqueue(item.EndPoint);
                    }
                }
            }

            CleanUp();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<Node<TKey, TValue, TEdge>>).GetEnumerator();
        }
    }
    [JsonObject(MemberSerialization.OptIn)]
    class Node<TKey,TValue, TEdge> : IID
    {
        public LinkedList<TKey> Previous = new LinkedList<TKey>();
        public int Cluster = -1;
        public int Distance = -1;
        public bool Seen = false;
        public TKey Key;

        [JsonProperty]
        public TValue Data;

        public List<Edge<TKey, TValue, TEdge>> Connections = new List<Edge<TKey, TValue, TEdge>>();
        public int ID { get; set; }
        public Node(TKey key, TValue data, params Edge<TKey, TValue, TEdge>[] lst)
        {
            Key = key;
            Data =  data;
            if(lst != null)
                Connections = lst.ToList(); ;
        }
        public void ConnectByDirectionally(Node<TKey, TValue, TEdge> node,TEdge data)
        {
            Connections.Add(new Edge< TKey, TValue, TEdge >(data,node));
            node.Connections.Add(new Edge<TKey, TValue, TEdge>(data, this));
        }
        public void ConnectOneWay(Node<TKey, TValue, TEdge> node, TEdge data)
        {
            Connections.Add(new Edge<TKey, TValue, TEdge>(data, node));
        }
        public void RemoveConnection(TKey t)
        {
            for (int i = 0; i < Connections.Count; i++)
            {
                if (Connections[i].EndPoint.Key.Equals(t))
                    Connections[i].Removed = true; ;
            }
        }
        public void Refresh()
        {
            var lst = new LinkedList<Edge<TKey, TValue, TEdge>>();
            foreach (var item in Connections)
            {
                if (!item.Removed)
                    lst.AddLast(item);
            }

            Connections = lst.ToList();
        }
        public override string ToString()
        {
            return $" ( {Key} : {Data} ) ";
        }
    }
    class Comp<a,b,c> : IComparer<Node<a, b, c>>
    {
        
        int IComparer<Node<a, b, c>>.Compare(Node<a, b, c> x, Node<a, b, c> y)
        {
            return x.Distance.CompareTo(y.Distance);
        }
    }
    class Edge<TKey, TValue, TEdge> : IComparable<Edge<TKey, TValue, TEdge>>
    {
        public bool Passed = false;
        public bool Removed = false;
        public TEdge Data;
        public Node<TKey, TValue, TEdge> EndPoint;
        public Edge(TEdge data, Node<TKey, TValue, TEdge> end)
        {
            Data = data;
            EndPoint = end;
        }
        int IComparable<Edge<TKey, TValue, TEdge>>.CompareTo(Edge<TKey, TValue, TEdge> other)
        {
            return this.EndPoint.Distance.CompareTo(other.EndPoint.Distance);
        }
        public override string ToString()
        {
            return $" ( {EndPoint.Key} , {Data} ) ";
        }
    }
    class DGraph<TKey, TValue, TEdge> : IEnumerable<TValue>
    {
        Dictionary<TKey, Node<TKey, TValue, TEdge>> nodes = new Dictionary<TKey, Node<TKey, TValue, TEdge>>();

        public TValue this[TKey key]
        {
            get
            {
                return nodes[key].Data;
            }
        }

        public void AddNode(TKey key, TValue value)
        {
            Node<TKey, TValue, TEdge> node = new Node<TKey, TValue, TEdge>(key, value, null);

            nodes.Add(key, node);
        }

        public void ConnectNodes(TKey t1, TKey t2, TEdge edge)
        {
            nodes[t1].ConnectOneWay(nodes[t2], edge);
        }

        public bool IsCyclic()
        {
            List<TKey> t = null;
            foreach (var item in nodes)
            {
                if (DFSHasPath(item.Key, item.Key, ref t))
                {
                    t.Reverse();
                    foreach (var i in t)
                        Console.WriteLine(i);
                    return true;
                }
            }

            return false;
        }


        public TKey[] TopologicalOrdering()
        {
            int id = nodes.Count - 1;
            TKey[] t = new TKey[nodes.Count];

            foreach (var elem in nodes)
            {
                if (!elem.Value.Seen)
                    TopologicalOrdering(elem.Value, ref id, t);
            }
            return t;
        }
        private void TopologicalOrdering(Node<TKey, TValue, TEdge> node, ref int i, TKey[] t)
        {
            node.Seen = true;
            foreach (var edge in node.Connections)
            {
                if (!edge.EndPoint.Seen)
                {
                    TopologicalOrdering(edge.EndPoint, ref i, t);
                }
            }
            t[i--] = node.Key;
        }

        public bool DFSHasPath(TKey source, TKey destination, ref List<TKey> lst)
        {
            Node<TKey, TValue, TEdge> src = nodes[source];
            src.Seen = true;

            foreach (var item in src.Connections)
            {
                if (item.EndPoint == nodes[destination])
                {
                    lst = new List<TKey>() { destination, source };
                    return true;
                }

                if (!item.EndPoint.Seen)
                {
                    if (DFSHasPath(item.EndPoint.Key, destination, ref lst))
                    {
                        lst.Add(source);
                        return true;
                    }
                }
            }

            return false;
        }

        public bool ContainsFullCycle(out TKey[] path)
        {
            path = new TKey[nodes.Count];
            path[0] = nodes.ElementAt(0).Key;
            return ContainsFullCycle(path, 1);
        }

        private bool ContainsFullCycle(TKey[] path, int pos)
        {
            if (pos == path.Length)
            {
                foreach (var item in nodes[path[pos - 1]].Connections)
                    if (item.EndPoint.Key.Equals(path[0]))
                        return true;
                return false;
            }
            Node<TKey, TValue, TEdge> temp = nodes[path[pos - 1]];
           
            foreach (var item in temp.Connections)
            {
                if (!path.Contains(item.EndPoint.Key))
                {
                    path[pos] = item.EndPoint.Key;
                    if (ContainsFullCycle(path, pos + 1))
                        return true;
                    path[pos] = default;
                }
            }
            return false;
        }

        private bool BFSCycle(TKey source)
        {
            CleanUp();
            Queue<Node<TKey, TValue, TEdge>> queue = new Queue<Node<TKey, TValue, TEdge>>();

            Node<TKey, TValue, TEdge> node = nodes[source];

            queue.Enqueue(node);
            node.Seen = true;

            while (queue.Count != 0)
            {
                Node<TKey, TValue, TEdge> temp = queue.Dequeue();

                Console.Write(temp.Data + " ");

                foreach (var item in temp.Connections)
                {
                    Console.Write(item.EndPoint.Data + " ");
                    if (!item.EndPoint.Seen)
                    {
                        item.EndPoint.Seen = true;
                        //item.Passed = true;
                        queue.Enqueue(item.EndPoint);
                    }
                    else if (item.EndPoint == node)
                        return true;

                }
                Console.WriteLine();
            }

            CleanUp();
            return false;
        }

        private void CleanUp()
        {
            foreach (var item in nodes)
            {
                item.Value.Seen = false;
                item.Value.Distance = 0;
                item.Value.Previous = new LinkedList<TKey>();
                foreach (var it in item.Value.Connections)
                    it.Passed = false;
            }
        }

        public int ShortesPath(TKey source, TKey destination)
        {
            CleanUp();
            Queue<Node<TKey, TValue, TEdge>> queue = new Queue<Node<TKey, TValue, TEdge>>();

            Node<TKey, TValue, TEdge> src = nodes[source];
            Node<TKey, TValue, TEdge> des = nodes[destination];

            queue.Enqueue(src);
            src.Seen = true;
            src.Previous = new LinkedList<TKey>();
            //src.Previous.AddLast(src.Key);



            while (queue.Count != 0)
            {
                Node<TKey, TValue, TEdge> temp = queue.Dequeue();


                Console.WriteLine($"Node {temp.Key}");


                foreach (var item in temp.Connections)
                {
                    Console.Write(item.EndPoint.Key + " ");
                    int dist;
                    if (item.Data is int i)
                        dist = temp.Distance + i;
                    else
                        throw new Exception("The data of roads is not numeric value");

                    if (!item.EndPoint.Seen)
                    {
                        item.EndPoint.Seen = true;
                        queue.Enqueue(item.EndPoint);
                        item.EndPoint.Distance = dist;
                        item.EndPoint.Previous = temp.Previous.Clone();
                        item.EndPoint.Previous.AddLast(temp.Key);

                    }
                    else if (item.EndPoint.Distance > dist)
                    {
                        item.EndPoint.Distance = dist;
                        item.EndPoint.Previous = temp.Previous.Clone();
                        item.EndPoint.Previous.AddLast(temp.Key);
                    }

                }
                Console.WriteLine();
            }

            int ans = nodes[destination].Distance;

            //foreach (var item in nodes[destination].Previous)
            //{
            //    Console.Write(item + " ");
            //}
            //Console.WriteLine(destination);

            CleanUp();
            return ans;
        }

        public int MinimumSpanTree()
        {
            int weight = 0;
            if (nodes.Count <= 1)
                return 0;

            CleanUp();
            Node<TKey, TValue, TEdge> beg = nodes.ElementAt(0).Value;
            LinkedList<Node<TKey, TValue, TEdge>> list = new LinkedList<Node<TKey, TValue, TEdge>>();
            list.AddLast(beg);
            Node<TKey, TValue, TEdge> temp = null;
            while (list.Count != nodes.Count)
            {
                int min = int.MaxValue;
                foreach (var item in list)
                {
                    Console.WriteLine(item.Key + "  ");
                    foreach (var connection in item.Connections)
                    {
                        if (connection.Data is int length)
                        {
                            if (min > length && !list.Contains(connection.EndPoint))
                            {
                                min = length;
                                temp = connection.EndPoint;
                            }
                        }
                        else
                            throw new Exception("Invalid data on the edges");
                    }
                }
                weight += min;
                list.AddLast(temp);
            }
            Console.WriteLine();
            foreach (var item in list)
            {
                Console.Write(item.Data + " ");
            }
            Console.WriteLine();
            return weight;
        }

        public void DFS(TKey source)
        {
            Stack<Node<TKey, TValue, TEdge>> stack = new Stack<Node<TKey, TValue, TEdge>>();
            stack.Push(nodes[source]);
            nodes[source].Seen = true;

            while (!stack.IsEmpty)
            {
                Node<TKey, TValue, TEdge> temp = stack.Pop();

                Console.WriteLine(temp.Data);

                foreach (var item in temp.Connections)
                {
                    if (!item.EndPoint.Seen)
                    {
                        item.EndPoint.Seen = true;
                        stack.Push(item.EndPoint);
                    }
                }

            }

        }

        IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
        {
            CleanUp();
            Queue<Node<TKey, TValue, TEdge>> queue = new Queue<Node<TKey, TValue, TEdge>>();

            Node<TKey, TValue, TEdge> node = nodes.ElementAt(0).Value;

            queue.Enqueue(node);
            node.Seen = true;

            while (queue.Count != 0)
            {
                Node<TKey, TValue, TEdge> temp = queue.Dequeue();

                yield return temp.Data;

                foreach (var item in temp.Connections)
                {
                    if (!item.EndPoint.Seen)
                    {
                        item.EndPoint.Seen = true;
                        queue.Enqueue(item.EndPoint);
                    }
                }
            }

            CleanUp();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<Node<TKey, TValue, TEdge>>).GetEnumerator();
        }
    }
   
    static class Extensions {

        public static LinkedList<T> Clone<T>(this LinkedList<T> t)
        {
            LinkedList<T> list = new LinkedList<T>();

            foreach (var item in t)
                list.AddLast(item);
            
            return list;
        }

    }
}
