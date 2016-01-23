#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

#endregion

namespace NVNC.Utils.ScreenTree
{
    public class QuadNode : IEquatable<QuadNode>, IComparable<QuadNode>
    {
        private static int _id;
        internal static int MIN_HEIGHT = 64;
        internal static int MIN_WIDTH = 64;

        //The 2500000000000000th prime number
        public static long Q = 75674484987354031L;
        public int[][] childrenData = new int[4][];

        public long[] childrenHashes = new long[4];
        public QuadNode[] childrenNodes = new QuadNode[4];
        public Rectangle2[] childrenRect = new Rectangle2[4];

        public int Id = _id++;

        public QuadNode(Rectangle2 bounds, int[] data)
        {
            Bounds = bounds;
            NodeData = data;
            GenerateChildren();
        }

        public Rectangle2 Bounds { get; set; }
        public int[] NodeData { get; set; }
        public long DataHash { get; internal set; }
        public QuadNode Parent { get; internal set; }

        public bool IsExpanded { get; internal set; }

        public QuadNode this[Direction direction]
        {
            get
            {
                switch (direction)
                {
                    case Direction.NW:
                        return childrenNodes[0];
                    case Direction.NE:
                        return childrenNodes[1];
                    case Direction.SW:
                        return childrenNodes[2];
                    case Direction.SE:
                        return childrenNodes[3];
                    default:
                        return null;
                }
            }
            set
            {
                switch (direction)
                {
                    case Direction.NW:
                        childrenNodes[0] = value;
                        break;
                    case Direction.NE:
                        childrenNodes[1] = value;
                        break;
                    case Direction.SW:
                        childrenNodes[2] = value;
                        break;
                    case Direction.SE:
                        childrenNodes[3] = value;
                        break;
                }
                if (value != null)
                    value.Parent = this;
            }
        }

        public bool CanExpand()
        {
            return Bounds.Width > MIN_WIDTH && Bounds.Height > MIN_HEIGHT;
        }

        public void CalculateHash()
        {
            if (!IsExpanded) return;
            for (var i = 0; i < 4; i++)
            {
                var li = i;
                var d = (Direction) li;
                this[d].CalculateHash();
                DataHash = (DataHash + this[d].DataHash)%Q;
            }
        }

        public void Expand()
        {
            if (CanExpand())
            {
                IsExpanded = true;
                //Thread[] cThreads = new Thread[4];
                for (var i = 0; i < 4; i++)
                {
                    var li = i;
                    var d = (Direction) li;
                    //cThreads[li] = new Thread(delegate()
                    //ThreadPool.QueueUserWorkItem(func =>
                    //{
                    this[d] = new QuadNode(childrenRect[li], childrenData[li]);
                    this[d].Expand();
                    //this.DataHash = (this.DataHash + this[d].DataHash) % Q; 
                    //});
                }
                /*
                foreach (Thread t in cThreads)
                {
                    t.Start();
                }
                for (int i = 0; i < 4; i++)
                {
                    cThreads[i].Join();
                    DataHash = (DataHash + this[(Direction) i].DataHash)%Q;
                }
                */
            }
            else
            {
                WaitHandle[] waitHandles =
                {
                    new ManualResetEvent(false),
                    new ManualResetEvent(false),
                    new ManualResetEvent(false),
                    new ManualResetEvent(false)
                };

                for (var i = 0; i < 4; i++)
                {
                    var li = i;
                    ThreadPool.QueueUserWorkItem(func =>
                    {
                        var occurances = new Dictionary<int, long>();
                        long h = 1;
                        long maxO = -1;
                        long maxV = -1;

                        for (long j = 0; j < childrenData[li].Length; j++)
                        {
                            var px = childrenData[li][j];
                            h = h*((px + j)%Q)%Q;

                            var val = px;
                            if (!occurances.ContainsKey(val))
                                occurances.Add(val, 0);
                            occurances[val]++;

                            if (occurances[val] > maxO)
                            {
                                maxO = occurances[val];
                                maxV = val;
                            }
                        }

                        childrenHashes[li] = h;
                        long diff = occurances.Count;

                        //Calculates the percentage of different pixels in the rectangle
                        //If it is less than 10 in 1024, the tile is considered to be filled with a solid color
                        //The solid color used for filling is the color which occured the most times
                        var percDiff = (float) diff/childrenData[li].Length;
                        if (percDiff < 0.01)
                        {
                            childrenRect[li].SetSolidColor((int) maxV);
                            childrenHashes[li] = (long) Math.Pow(maxV*maxO*diff, 3)%Q;
                            //idk if the previous hash would be better or this one
                        }
                        DataHash = (DataHash + childrenHashes[li])%Q;
                        ((ManualResetEvent) waitHandles[li]).Set();
                    });
                }
                WaitHandle.WaitAll(waitHandles);
            }
        }

        public void GenerateChildren()
        {
            var westX = Bounds.X; //Keep X and Y just in case, needed on client side
            var westY = Bounds.Y;
            var westWidth = Bounds.Width/2;
            var westHeight = Bounds.Height/2;

            var eastX = westX + westWidth;
            var eastY = westY;
            var eastWidth = Bounds.Width - westWidth;
            var eastHeight = Bounds.Height - westHeight;

            var nw = new Rectangle2(westX, westY, westWidth, westHeight);
            var nwd = PixelGrabber.CopyPixels(NodeData, Bounds.Width, 0, 0, westWidth, westHeight);
            childrenData[(int) Direction.NW] = nwd; //NodeData;
            childrenRect[(int) Direction.NW] = nw;

            var parentWidth = Bounds.Width;
            var scanline = parentWidth;
            var size = westWidth*westHeight;
            var jump = scanline - westWidth;
            var s = 0;
            var p = 0*scanline + 0;
            Trace.WriteLine("My offset: " + p);
            for (var i = 0; i < size; i++, s++, p++)
            {
                if (s == westWidth)
                {
                    s = 0;
                    p += jump;
                }
            }
            Trace.WriteLine("My end: " + --p);

            var ne = new Rectangle2(eastX, eastY, eastWidth, eastHeight);

            // TODO: Keep relative pixel start and end instead of copying the part of the array
            // The current implementation is a naive one and very slow, but it works as intended
            childrenData[(int) Direction.NE] = PixelGrabber.CopyPixels(NodeData, Bounds.Width, westWidth, 0, eastWidth,
                eastHeight);
            childrenRect[(int) Direction.NE] = ne;

            s = 0;
            p = 0*scanline + westWidth;
            Trace.WriteLine("My offset: " + p);
            for (var i = 0; i < size; i++, s++, p++)
            {
                if (s == westWidth)
                {
                    s = 0;
                    p += jump;
                }
            }
            Trace.WriteLine("My end: " + --p);


            var sw = new Rectangle2(westX, westY + westHeight, westWidth, westHeight);
            childrenData[(int) Direction.SW] = PixelGrabber.CopyPixels(NodeData, Bounds.Width, 0, 0 + westHeight,
                westWidth, westHeight);
            childrenRect[(int) Direction.SW] = sw;

            var se = new Rectangle2(eastX, eastY + eastHeight, eastWidth, eastHeight);
            childrenData[(int) Direction.SE] = PixelGrabber.CopyPixels(NodeData, Bounds.Width, westWidth, 0 + eastHeight,
                eastWidth, eastHeight);
            childrenRect[(int) Direction.SE] = se;
        }

        public static QuadNode EmptyNode()
        {
            return new QuadNode(new Rectangle2(0, 0, 2, 2), new[] {0, 0, 0, 0});
        }

        #region Equality

        public int CompareTo(QuadNode other)
        {
            return Id - other.Id;
        }

        public override string ToString()
        {
            return string.Format("{0}", Bounds);
        }

        public bool Equals(QuadNode other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            if (Bounds.IsSolidColor && other.Bounds.IsSolidColor)
                return Bounds.SolidColor == other.Bounds.SolidColor;

            return ArrayEquals(NodeData, other.NodeData);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((QuadNode) obj);
        }

        private bool ArrayEquals<T>(T[] a, T[] b)
        {
            if (a == null || b == null) return false;
            if (a.Length != b.Length) return false;
            for (var i = 0; i < a.Length; i++)
            {
                if (!a[i].Equals(b[i])) return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return (int) (Bounds.GetHashCode()*(DataHash%Q)%Q);
        }

        #endregion
    }
}