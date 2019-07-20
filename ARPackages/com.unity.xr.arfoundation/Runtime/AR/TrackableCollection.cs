using System;
using System.Collections.Generic;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.ARFoundation
{
    /// <summary>
    /// A collection for <see cref="ARTrackable{TSessionRelativeData, TTrackable}"/>s.
    /// This collection implements an IEnumerable-like interface which can be used
    /// in a <c>foreach</c> statement.
    /// </summary>
    /// <typeparam name="TTrackable">The concrete <see cref="ARTrackable{TSessionRelativeData, TTrackable}"/>.</typeparam>
    public struct TrackableCollection<TTrackable> 
    {
        /// <summary>
        /// Creates an <c>Enumerator</c> for this collection.
        /// </summary>
        /// <returns></returns>
        public Enumerator GetEnumerator()
        {
            return new Enumerator(m_Trackables);
        }

        /// <summary>
        /// Constructs a <see cref="TrackableCollection{TSessionRelativeData, TTrackable}"/>.
        /// </summary>
        /// <param name="trackables"></param>
        public TrackableCollection(Dictionary<TrackableId, TTrackable> trackables)
        {
            if (trackables == null)
                throw new ArgumentNullException("trackables");

            m_Trackables = trackables;
        }

        /// <summary>
        /// Returns the number of trackables in this collection.
        /// </summary>
        public int count
        {
            get
            {
                if (m_Trackables == null)
                    throw new InvalidOperationException("This collection has not been initialized.");

                return m_Trackables.Count;
            }
        }

        /// <summary>
        /// Retrieves a <c>TTrackable</c>s by <c>TrackableId</c>.
        /// </summary>
        /// <param name="trackableId">The trackable id associated with the trackable to retrieve.</param>
        /// <returns>The <c>TTrackable</c>s if present. Throws <c>KeyNotFoundException</c> if the <paramref name="trackableId"/> is not found.</returns>
        public TTrackable this[TrackableId trackableId]
        {
            get
            {
                if (m_Trackables == null)
                    throw new InvalidOperationException("This collection has not been initialized.");

                try
                {
                    return m_Trackables[trackableId];
                }
                catch (KeyNotFoundException e)
                {
                    throw new KeyNotFoundException(
                        string.Format("Trackable with id {0} does not exist in this collection.", trackableId),
                        e);
                }
            }
        }

        /// <summary>
        /// Attempts to retrieve a trackable by <c>TrackableId</c>.
        /// </summary>
        /// <param name="trackableId">The trackable id associated with the trackable to retrieve.</param>
        /// <param name="trackable">Set to the trackable with <paramref name="trackableId"/>, if present in the collection.</param>
        /// <returns><c>true</c> if the trackable with <paramref name="trackableId"/> exists. <c>false</c> otherwise.</returns>
        public bool TryGetTrackable(TrackableId trackableId, out TTrackable trackable)
        {
            if (m_Trackables == null)
                throw new InvalidOperationException("This collection has not been initialized.");

            return m_Trackables.TryGetValue(trackableId, out trackable);
        }

        /// <summary>
        /// An <c>Enumerator</c> for <c>TTrackable</c>s.
        /// </summary>
        public struct Enumerator
        {
            /// <summary>
            /// Constructs an <c>Enumerator</c> for use with <c>TTrackable</c>s.
            /// </summary>
            /// <param name="trackables">A <c>Dictionary</c> of <c>TTrackable</c>s.</param>
            public Enumerator(Dictionary<TrackableId, TTrackable> trackables)
            {
                if (trackables == null)
                    throw new ArgumentNullException("trackables");

                m_Enumerator = trackables.GetEnumerator();
            }

            /// <summary>
            /// Moves to the next trackable in the collection.
            /// </summary>
            /// <returns><c>true</c> if the next trackable is valid, or <c>false</c> if it is the end of the collection.</returns>
            public bool MoveNext()
            {
                return m_Enumerator.MoveNext();
            }

            /// <summary>
            /// The current value in the collection.
            /// </summary>
            public TTrackable Current
            {
                get
                {
                    return m_Enumerator.Current.Value;
                }
            }

            /// <summary>
            /// Releases all resources used by this <c>Enumerator</c>.
            /// </summary>
            public void Dispose()
            {
                m_Enumerator.Dispose();
            }

            Dictionary<TrackableId, TTrackable>.Enumerator m_Enumerator;

        }

        Dictionary<TrackableId, TTrackable> m_Trackables;
    }
}
