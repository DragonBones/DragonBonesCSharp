namespace DragonBones
{
    public class Point
    {
        public float x = 0.0f;
        public float y = 0.0f;

        public Point()
        {
        }

        public void CopyFrom(Point value)
        {
            this.x = value.x;
            this.y = value.y;
        }

        public void Clear()
        {
            this.x = this.y = 0.0f;
        }
    }
}
