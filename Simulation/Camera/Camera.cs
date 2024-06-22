using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace Game.ViewTools
{
    public class Camera
    {
        public Vector2f Position;

        public FloatRect ViewPort;

        public float Rotation { get; set; }

        public float Zoom { get; set; } = 1;

        private View view;

        public Camera(FloatRect configuration)
        {
            Position = new Vector2f(SimulationConfig.Width / 2, SimulationConfig.Height / 2);

            this.view = new View(new FloatRect(
                new Vector2f(0, 0),
                new Vector2f(SimulationConfig.Width, SimulationConfig.Height)));

            this.ViewPort = configuration;
        }

        public void Update(float deltaT)
        {
            // If we dont have camera movement enabled, return immediately
            if (!SimulationConfig.AllowCameraMovement)
            {
                return;
            }

            // Camera movement is handled via an offset from the current position for simplicity.
            var ratio = SimulationConfig.Width / (float)SimulationConfig.Height;
            var offset = new Vector2f(0, 0);

            // Translation
            if (Keyboard.IsKeyPressed(SimulationConfig.PanUp))
            {
                offset.Y -= SimulationConfig.CameraMovementSpeed * deltaT;
            }

            if (Keyboard.IsKeyPressed(SimulationConfig.PanLeft))
            {
                offset.X -= SimulationConfig.CameraMovementSpeed * deltaT / ratio;
            }

            if (Keyboard.IsKeyPressed(SimulationConfig.PanDown))
            {
                offset.Y += SimulationConfig.CameraMovementSpeed * deltaT;
            }

            if (Keyboard.IsKeyPressed(SimulationConfig.PanRight))
            {
                offset.X += SimulationConfig.CameraMovementSpeed * deltaT / ratio;
            }

            // Zoom
            if (Keyboard.IsKeyPressed(SimulationConfig.ZoomIn))
            {
                this.Zoom += SimulationConfig.CameraZoomSpeed * deltaT;
            }
            else if (Keyboard.IsKeyPressed(SimulationConfig.ZoomOut))
            {
                this.Zoom -= SimulationConfig.CameraZoomSpeed * deltaT;
            }
            else
            {
                this.Zoom = 1;
            }

            // Rotation
            if (Keyboard.IsKeyPressed(SimulationConfig.RotateLeft))
            {
                this.Rotation -= SimulationConfig.CameraRotationSpeed * deltaT;
            }

            if (Keyboard.IsKeyPressed(SimulationConfig.RotateRight))
            {
                this.Rotation += SimulationConfig.CameraRotationSpeed * deltaT;
            }

            // Update all the things we just calculated.
            this.Position += offset;
            
            this.view.Rotation = this.Rotation;

            this.view.Viewport = this.ViewPort;

            this.SetCentre(this.Position, 0.2f);

            this.view.Zoom(this.Zoom);
        }

        public void SetCentre(Vector2f centre, float proportion = 1)
        {
            var difference = (this.view.Center - centre) * proportion;

            this.view.Center = this.view.Center -= difference;
        }

        public void ScaleToWindow(float width, float height)
        {
            var viewAspect = GetDesiredAspectRatio();
            var windowAspect = width / height;

            this.ViewPort = new FloatRect(0, 0, 1, 1);
            if(windowAspect > viewAspect)
            {
                this.ViewPort.Width = viewAspect / windowAspect;
                this.ViewPort.Left = (1f - this.ViewPort.Width) / 2f;
            }
            else
            {
                this.ViewPort.Height = windowAspect / viewAspect;
                this.ViewPort.Top = (1 - this.ViewPort.Height) / 2f;
            }
        }

        private float GetDesiredAspectRatio()
        {
            return SimulationConfig.Width / (float)SimulationConfig.Height;
        }

        public View GetView()
        {
            return view;
        }
    }
}
