namespace DragonBones
{
    public class Rectangle
    {
        public float x;
        public float y;
        public float width;
        public float height;

        public Rectangle()
        {
        }

        public void CopyFrom(Rectangle value)
        {
            this.x = value.x;
            this.y = value.y;
            this.width = value.width;
            this.height = value.height;
        }

        public void Clear()
        {
            this.x = this.y = 0.0f;
            this.width = this.height = 0.0f;
        }
    }
}
