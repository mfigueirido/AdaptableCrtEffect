using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AdaptableCrtEffect
{
    public static class QuadRenderer
    {
        // This are the vertex numbers 
        // and positions if drawing as 
        // a fullscreen quad:
        //    0--------------1
        //    |-1,1       1,1|
        //    |              |
        //    |              |
        //    |              |
        //    |-1,-1     1,-1|
        //    3--------------2
        //
        // This are the vertex numbers 
        // and texture coordinates for 
        // a textured quad:
        //    0--------------1
        //    |0,0        1,0|
        //    |              |
        //    |              |
        //    |              |
        //    |0,1        1,1|
        //    3--------------2
        ///////////////////////////////

        static GraphicsDevice _device;

        static VertexPositionTexture[] _fitViewportVertices = new VertexPositionTexture[]
        {
            new VertexPositionTexture(new Vector3(-1, 1, 0), new Vector2(0, 0)),
            new VertexPositionTexture(new Vector3(1, 1, 0), new Vector2(1, 0)),
            new VertexPositionTexture(new Vector3(1, -1, 0), new Vector2(1, 1)),
            new VertexPositionTexture(new Vector3(-1, -1, 0), new Vector2(0, 1))
        };

        static VertexPositionTexture[] _customVertices = new VertexPositionTexture[]
        {
            new VertexPositionTexture(new Vector3(0, 0, 0), new Vector2(0, 0)),
            new VertexPositionTexture(new Vector3(0, 0, 0), new Vector2(1, 0)),
            new VertexPositionTexture(new Vector3(0, 0, 0), new Vector2(1, 1)),
            new VertexPositionTexture(new Vector3(0, 0, 0), new Vector2(0, 1))
        };

        static VertexPositionColorTexture[] _fitViewportColorVertices = new VertexPositionColorTexture[]
        {
            new VertexPositionColorTexture(new Vector3(-1, 1, 0), Color.White, new Vector2(0, 0)),
            new VertexPositionColorTexture(new Vector3(1, 1, 0), Color.White, new Vector2(1, 0)),
            new VertexPositionColorTexture(new Vector3(1, -1, 0), Color.White, new Vector2(1, 1)),
            new VertexPositionColorTexture(new Vector3(-1, -1, 0), Color.White, new Vector2(0, 1))
        };

        static VertexPositionColorTexture[] _customColorVertices = new VertexPositionColorTexture[]
        {
            new VertexPositionColorTexture(new Vector3(0, 0, 0), Color.White, new Vector2(0, 0)),
            new VertexPositionColorTexture(new Vector3(0, 0, 0), Color.White, new Vector2(1, 0)),
            new VertexPositionColorTexture(new Vector3(0, 0, 0), Color.White, new Vector2(1, 1)),
            new VertexPositionColorTexture(new Vector3(0, 0, 0), Color.White, new Vector2(0, 1))
        };

        static short[] _indices = { 0, 1, 2, 2, 3, 0 };
        static ushort[] _bufferedIndices = { 0, 1, 2, 2, 3, 0 };

        static VertexBuffer _vertexBuffer;
        static VertexBuffer _vertexColorBuffer;
        static IndexBuffer _indexBuffer;

        public static bool UseVertexColor { get; set; }

        static QuadRenderer()
        {
            _device = GameHelper.GraphicsDevice;

            _vertexBuffer = new VertexBuffer(_device, typeof(VertexPositionTexture), _fitViewportVertices.Length, BufferUsage.None);

            _vertexColorBuffer = new VertexBuffer(_device, typeof(VertexPositionColorTexture), _fitViewportColorVertices.Length, BufferUsage.None);

            _indexBuffer = new IndexBuffer(_device, typeof(ushort), _bufferedIndices.Length, BufferUsage.None);
            _indexBuffer.SetData(_bufferedIndices);
        }

        public static void SetBuffers(bool useCustomVertices)
        {
            _device.Indices = _indexBuffer;

            if (UseVertexColor)
            {
                if (useCustomVertices)
                    _vertexColorBuffer.SetData(_customColorVertices);
                else
                    _vertexColorBuffer.SetData(_fitViewportColorVertices);

                _device.SetVertexBuffer(_vertexColorBuffer);

            }
            else
            {
                if (useCustomVertices)
                    _vertexBuffer.SetData(_customVertices);
                else
                    _vertexBuffer.SetData(_fitViewportVertices);

                _device.SetVertexBuffer(_vertexBuffer);
            }
        }

        public static void UnsetBuffers()
        {
            _device.Indices = null;
            _device.SetVertexBuffer(null);
        }

        public static void UpdateCustomVerticesPosition(int destinationWidth, int destinationHeight)
        {
            float viewportWidth = _device.Viewport.Width;
            float viewportHeight = _device.Viewport.Height;
            float horizontalPosition = 1f + ((destinationWidth - viewportWidth) / viewportWidth);
            float verticalPosition = -1f - ((destinationHeight - viewportHeight) / viewportHeight);

            if (UseVertexColor)
            {
                _customColorVertices[0].Position.X = -1f;
                _customColorVertices[0].Position.Y = 1f;

                _customColorVertices[1].Position.X = horizontalPosition;
                _customColorVertices[1].Position.Y = 1f;

                _customColorVertices[2].Position.X = horizontalPosition;
                _customColorVertices[2].Position.Y = verticalPosition;

                _customColorVertices[3].Position.X = -1f;
                _customColorVertices[3].Position.Y = verticalPosition;
            }
            else
            {
                _customVertices[0].Position.X = -1f;
                _customVertices[0].Position.Y = 1f;

                _customVertices[1].Position.X = horizontalPosition;
                _customVertices[1].Position.Y = 1f;

                _customVertices[2].Position.X = horizontalPosition;
                _customVertices[2].Position.Y = verticalPosition;

                _customVertices[3].Position.X = -1f;
                _customVertices[3].Position.Y = verticalPosition;
            }
        }

        public static void UpdateCustomVerticesPosition(Vector2 topLeft, Vector2 bottomRight)
        {
            if (UseVertexColor)
            {
                _customColorVertices[0].Position.X = topLeft.X;
                _customColorVertices[0].Position.Y = topLeft.Y;

                _customColorVertices[1].Position.X = bottomRight.X;
                _customColorVertices[1].Position.Y = topLeft.Y;

                _customColorVertices[2].Position.X = bottomRight.X;
                _customColorVertices[2].Position.Y = bottomRight.Y;

                _customColorVertices[3].Position.X = topLeft.X;
                _customColorVertices[3].Position.Y = bottomRight.Y;
            }
            else
            {
                _customVertices[0].Position.X = topLeft.X;
                _customVertices[0].Position.Y = topLeft.Y;

                _customVertices[1].Position.X = bottomRight.X;
                _customVertices[1].Position.Y = topLeft.Y;

                _customVertices[2].Position.X = bottomRight.X;
                _customVertices[2].Position.Y = bottomRight.Y;

                _customVertices[3].Position.X = topLeft.X;
                _customVertices[3].Position.Y = bottomRight.Y;
            }
        }

        public static void UpdateCustomVerticesTexCoord(Vector2 topLeft, Vector2 bottomRight)
        {
            if (UseVertexColor)
            {
                _customColorVertices[0].TextureCoordinate.X = topLeft.X;
                _customColorVertices[0].TextureCoordinate.Y = topLeft.Y;

                _customColorVertices[1].TextureCoordinate.X = bottomRight.X;
                _customColorVertices[1].TextureCoordinate.Y = topLeft.Y;

                _customColorVertices[2].TextureCoordinate.X = bottomRight.X;
                _customColorVertices[2].TextureCoordinate.Y = bottomRight.Y;

                _customColorVertices[3].TextureCoordinate.X = topLeft.X;
                _customColorVertices[3].TextureCoordinate.Y = bottomRight.Y;
            }
            else
            {
                _customVertices[0].TextureCoordinate.X = topLeft.X;
                _customVertices[0].TextureCoordinate.Y = topLeft.Y;

                _customVertices[1].TextureCoordinate.X = bottomRight.X;
                _customVertices[1].TextureCoordinate.Y = topLeft.Y;

                _customVertices[2].TextureCoordinate.X = bottomRight.X;
                _customVertices[2].TextureCoordinate.Y = bottomRight.Y;

                _customVertices[3].TextureCoordinate.X = topLeft.X;
                _customVertices[3].TextureCoordinate.Y = bottomRight.Y;
            }
        }

        public static void RenderBuffered()
        {
            _device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 4, 0, 2);
        }

        public static void RenderFitViewport()
        {
            if (UseVertexColor)
                _device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, _fitViewportColorVertices, 0, 4, _indices, 0, 2);
            else
                _device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, _fitViewportVertices, 0, 4, _indices, 0, 2);
        }

        public static void RenderCustom()
        {
            if (UseVertexColor)
                _device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, _customColorVertices, 0, 4, _indices, 0, 2);
            else
                _device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, _customVertices, 0, 4, _indices, 0, 2);
        }
    }
}
