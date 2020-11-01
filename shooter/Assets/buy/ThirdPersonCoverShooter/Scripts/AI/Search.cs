using System.Collections.Generic;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Information about a searchable position.
    /// </summary>
    public struct SearchPoint
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector3 ApproachPosition;
        public bool HasNormal;
        public float Visibility;
        public int Left;
        public int Right;
        public bool RequiresReaching;

        public SearchPoint(Vector3 position, Vector3 approachPosition, Vector3 normal, bool requiresReaching)
        {
            Position = position;
            ApproachPosition = approachPosition;
            Normal = normal;
            HasNormal = true;
            Visibility = 9999999;
            Left = -1;
            Right = -1;
            RequiresReaching = requiresReaching;
        }

        public SearchPoint(Vector3 position, Vector3 normal, bool requiresReaching)
        {
            Position = position;
            ApproachPosition = position;
            Normal = normal;
            HasNormal = true;
            Visibility = 9999999;
            Left = -1;
            Right = -1;
            RequiresReaching = requiresReaching;
        }

        public SearchPoint(Vector3 position, bool requiresReaching)
        {
            Position = position;
            ApproachPosition = position;
            Normal = Vector3.zero;
            HasNormal = false;
            Visibility = 9999999;
            Left = -1;
            Right = -1;
            RequiresReaching = requiresReaching;
        }

        public void CalcVisibility(float maxDistance, bool isAlerted)
        {
            Visibility = Util.GetViewDistance(Position, maxDistance, isAlerted);
        }
    }

    public class SearchPointData
    {
        public List<SearchPoint> Points;

        public SearchPointData()
        {
            Points = new List<SearchPoint>();
        }

        public void LinkLeft(int left, int middle)
        {
            var point = Points[left];
            point.Right = middle;
            Points[left] = point;

            point = Points[middle];
            point.Left = left;
            Points[middle] = point;
        }

        public void LinkRight(int middle, int right)
        {
            var point = Points[middle];
            point.Right = right;
            Points[middle] = point;

            point = Points[right];
            point.Left = middle;
            Points[right] = point;
        }


        public int Add(SearchPoint point)
        {
            Points.Add(point);
            return Points.Count - 1;
        }

        public void Clear()
        {
            Points.Clear();
        }
    }

    public struct SearchBlock
    {
        public bool Empty
        {
            get { return Indices.Count == 0; }
        }

        public int Count
        {
            get { return Indices.Count; }
        }

        public SearchPointData Data;
        public List<int> Indices;
        public List<int> InvestigatedIndices;
        public Vector3 Center;
        public Vector3 Sum;
        public int Index;

        public SearchBlock(SearchPointData data)
        {
            Data = data;
            Indices = new List<int>();
            InvestigatedIndices = new List<int>();
            Center = Vector3.zero;
            Sum = Vector3.zero;
            Index = 0;
        }

        public void Investigate(int index)
        {
            InvestigatedIndices.Add(Indices[index]);
            Indices.RemoveAt(index);
        }

        public SearchPoint Get(int index)
        {
            return Data.Points[Indices[index]];
        }

        public void Add(int index)
        {
            Indices.Add(index);

            Sum += Data.Points[index].Position;
            Center = Sum / Indices.Count;
        }

        public bool IsClose(SearchPoint point, float threshold, float middleThreshold)
        {
            if (Vector3.Distance(Center, point.Position) < threshold)
                return true;

            foreach (var i in Indices)
                if (Vector3.Distance(Data.Points[i].Position, point.Position) < threshold)
                    return true;

            return false;
        }

        public void Clear()
        {
            Indices.Clear();
            InvestigatedIndices.Clear();
            Center = Vector3.zero;
            Sum = Vector3.zero;
        }
    }

    public class SearchBlockCache
    {
        private List<SearchBlock> _cache = new List<SearchBlock>();
        private SearchPointData _points;

        public SearchBlockCache(SearchPointData points)
        {
            _points = points;
        }

        public void Give(SearchBlock block)
        {
            _cache.Add(block);
        }

        public SearchBlock Take()
        {
            if (_cache.Count == 0)
                return new SearchBlock(_points);
            else
            {
                var block = _cache[_cache.Count - 1];
                _cache.RemoveAt(_cache.Count - 1);

                block.Clear();

                return block;
            }
        }
    }

    /// <summary>
    /// Information about an already investigated position.
    /// </summary>
    public struct InvestigatedPoint
    {
        public Vector3 Position;
        public float Time;

        public InvestigatedPoint(Vector3 position)
        {
            Position = position;
            Time = UnityEngine.Time.timeSinceLevelLoad;
        }
    }
}
