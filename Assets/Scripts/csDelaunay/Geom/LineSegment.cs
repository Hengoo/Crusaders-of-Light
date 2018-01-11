using System;
using System.Collections;
using System.Collections.Generic;

namespace csDelaunay {

	public class LineSegment : IEquatable<LineSegment>
    {

		public static List<LineSegment> VisibleLineSegments(List<Edge> edges) {
			List<LineSegment> segments = new List<LineSegment>();

			foreach (Edge edge in edges) {
				if (edge.Visible()) {
					Vector2f p1 = edge.ClippedEnds[LR.LEFT];
					Vector2f p2 = edge.ClippedEnds[LR.RIGHT];
					segments.Add(new LineSegment(p1,p2));
				}
			}

			return segments;
		}

		public static float CompareLengths_MAX(LineSegment segment0, LineSegment segment1) {
			float length0 = (segment0.p0 - segment0.p1).magnitude;
			float length1 = (segment1.p0 - segment1.p1).magnitude;
			if (length0 < length1) {
				return 1;
			}
			if (length0 > length1) {
				return -1;
			}
			return 0;
		}

		public static float CompareLengths(LineSegment edge0, LineSegment edge1) {
			return - CompareLengths_MAX(edge0, edge1);
		}

        public override bool Equals(object obj)
        {
            return Equals(obj as LineSegment);
        }

        public bool Equals(LineSegment other)
        {
            return other != null &&
                   EqualityComparer<Vector2f>.Default.Equals(p0, other.p0) &&
                   EqualityComparer<Vector2f>.Default.Equals(p1, other.p1);
        }

        public override int GetHashCode()
        {
            var hashCode = -1057177201;
            hashCode = hashCode * -1521134295 + EqualityComparer<Vector2f>.Default.GetHashCode(p0);
            hashCode = hashCode * -1521134295 + EqualityComparer<Vector2f>.Default.GetHashCode(p1);
            return hashCode;
        }

        public Vector2f p0;
		public Vector2f p1;

		public LineSegment (Vector2f p0, Vector2f p1) {
			this.p0 = p0;
			this.p1 = p1;
		}

        public static bool operator ==(LineSegment segment1, LineSegment segment2)
        {
            return EqualityComparer<LineSegment>.Default.Equals(segment1, segment2);
        }

        public static bool operator !=(LineSegment segment1, LineSegment segment2)
        {
            return !(segment1 == segment2);
        }
    }
}