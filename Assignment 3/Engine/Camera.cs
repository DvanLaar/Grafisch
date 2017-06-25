using OpenTK;

namespace rasterizer
{
    class Camera
    {
        const float Near = 0.1f, Far = 1000f;

        public Vector3 Position;
        private Quaternion rotation;
        private Matrix4 rotationMatrix;

        public Matrix4 Matrix { get { return rotationMatrix; } }

        public Camera(Vector3 position, Quaternion rotation)
        {
            Position = position;
            this.rotation = rotation;
            UpdateMatrix();
        }

        /// <summary>
        /// Rotates and translates the camera.
        /// It first rotates around the Y-axis with rotation.X degrees.
        /// After that, it will rotate around its own X-axis with rotation.Y degrees.
        /// After this, it will apply the translation to its own coordinate system.
        /// </summary>
        /// <param name="rotation">degrees of rotation around (Y-axis, X-axis)</param>
        /// <param name="translation">translation of the camera in 3 dimensions of its own coordinate system.</param>
        public void AddTransformation(Vector2 rotation, Vector3 translation)
        {
            if (rotation != Vector2.Zero)
            {
                Quaternion deltaY = Quaternion.FromAxisAngle(Vector3.UnitY, rotation.X);
                Vector3 dirX = deltaY * this.rotation * Vector3.UnitX;
                Quaternion deltaX = Quaternion.FromAxisAngle(dirX, rotation.Y);
                this.rotation = deltaX * deltaY * this.rotation;
            }
            if (translation != Vector3.Zero)
                Position += this.rotation * translation;

            // Update the matrix only once, so only if we are done after the rotation.
            if (rotation != Vector2.Zero || translation != Vector3.Zero)
                UpdateMatrix();
        }

        /// <summary>
        /// Updates the matrix belonging to this rotation.
        /// Matrixes are needed to concatenate transformations.
        /// However, quaternions are nicer to work with, while concatenating only rotations.
        /// That's why we have made it this way.
        /// </summary>
        private void UpdateMatrix()
        {
            Matrix4 rot = Matrix4.CreateFromQuaternion(rotation);
            rot.Transpose();
            Matrix4 trans = Matrix4.CreateTranslation(-Position);
            Matrix4 persp = Matrix4.CreatePerspectiveFieldOfView(1.2f, 1.3f, Near, Far);
            rotationMatrix = trans * rot * persp;
        }
    }
}
