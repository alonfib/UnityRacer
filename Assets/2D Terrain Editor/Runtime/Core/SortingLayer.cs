using System;
using UnityEngine;

namespace T2D
{
    /// <summary>
    /// Custom data structure of <see cref="Renderer"/> sorting layer and order values
    /// </summary>
    [Serializable]
    public struct SortingLayer : IEquatable<SortingLayer>
    {
        /// <summary>
        /// Sorting Layer order
        /// </summary>
        public int Order;

        /// <summary>
        /// Sorting Layer unique ID
        /// </summary>
        public int Id;

        /// <summary>
        /// Sorting Layer name
        /// </summary>
        public string Name
        {
            get => UnityEngine.SortingLayer.IDToName(Id);
            set => Id = UnityEngine.SortingLayer.NameToID(value);
        }


        public SortingLayer(int layerId, int order)
        {
            Id = layerId;
            Order = order;
        }

        public SortingLayer(string layerName, int order)
        {
            Id = UnityEngine.SortingLayer.NameToID(layerName);
            Order = order;
        }


        /// <summary>
        /// Applies Sorting Layer ID and Order to provided <see cref="Renderer"/>
        /// </summary>
        /// <param name="renderer"></param>
        public void SetToRenderer(Renderer renderer)
        {
            if (renderer == null)
                return;

            renderer.sortingLayerID = Id;
            renderer.sortingOrder = Order;
            renderer.sortingLayerName = Name;
        }

        public bool Equals(SortingLayer other)
        {
            return Order == other.Order && Id == other.Id;
        }


        /// <summary>
        /// Returns Sorting Layer ID and Order from given <see cref="Renderer"/>
        /// </summary>
        /// <param name="renderer"></param>
        /// <returns></returns>
        public static SortingLayer GetFromRenderer(Renderer renderer)
        {
            return renderer == null ? new SortingLayer(0, 0) : new SortingLayer(renderer.sortingOrder, renderer.sortingLayerID);
        }

        public static implicit operator SortingLayer(UnityEngine.SortingLayer sr) => new SortingLayer(sr.value, sr.id);
    }
}