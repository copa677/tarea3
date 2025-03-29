using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Linq.Expressions;

namespace OpenTKCubo3D
{
    class Program : GameWindow
    {
        private float _cameraAngleY;
        private float _cameraAngleX;
        private float _cameraDistance = 20.0f;
        //private int _vertexBufferObject;
        //private int _vertexArrayObject;
        private int _shaderProgram;
        private Matrix4 _view;
        private Matrix4 _projection;
        private List<Objeto3D> objetos = new List<Objeto3D>();

        public Program(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings, List<Objeto3D> objetos)
            : base(gameWindowSettings, nativeWindowSettings)
        {
            this.objetos = objetos;
        }

        protected override void OnLoad()
        {
            base.OnLoad();
            GL.ClearColor(0.1f, 0.1f, 0.2f, 1.0f);
            GL.Enable(EnableCap.DepthTest);

            foreach (var obj in objetos)
            {
                obj.Inicializar(); // Método nuevo para configurar VAO/VBO
            }

            // Compilar shaders
            string vertexShaderSource = @"
                #version 330 core
                layout(location = 0) in vec3 aPosition;
                layout(location = 1) in vec3 aColor;
                out vec3 fragColor;
                uniform mat4 model;
                uniform mat4 view;
                uniform mat4 projection;
                void main()
                {
                    gl_Position = projection * view * model * vec4(aPosition, 1.0);
                    fragColor = aColor;
                }
            ";

            string fragmentShaderSource = @"
                #version 330 core
                in vec3 fragColor;
                out vec4 color;
                void main()
                {
                    color = vec4(fragColor, 1.0);
                }
            ";

            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, vertexShaderSource);
            GL.CompileShader(vertexShader);

            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fragmentShaderSource);
            GL.CompileShader(fragmentShader);

            _shaderProgram = GL.CreateProgram();
            GL.AttachShader(_shaderProgram, vertexShader);
            GL.AttachShader(_shaderProgram, fragmentShader);
            GL.LinkProgram(_shaderProgram);

            GL.DetachShader(_shaderProgram, vertexShader);
            GL.DetachShader(_shaderProgram, fragmentShader);
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            // Configurar la vista y la proyección
            _view = Matrix4.LookAt(new Vector3(5, 5, 12), Vector3.Zero, Vector3.UnitY);
            _projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, Size.X / (float)Size.Y, 0.1f, 100f);
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, Size.X, Size.Y);
            _projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, Size.X / (float)Size.Y, 0.1f, 100f);
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
            var input = KeyboardState;

            if (input.IsKeyDown(Keys.Escape)) Close();

            float rotationSpeed = 0.002f;

            // Rotar cámara con flechas
            if (input.IsKeyDown(Keys.Left)) _cameraAngleY -= rotationSpeed;
            if (input.IsKeyDown(Keys.Right)) _cameraAngleY += rotationSpeed;
            if (input.IsKeyDown(Keys.Up)) _cameraAngleX -= rotationSpeed;
            if (input.IsKeyDown(Keys.Down)) _cameraAngleX += rotationSpeed;

            // Limitar ángulo vertical para evitar volteretas
            _cameraAngleX = MathHelper.Clamp(_cameraAngleX, -MathHelper.PiOver2 + 0.1f, MathHelper.PiOver2 - 0.1f);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.UseProgram(_shaderProgram);

            // Calcular posición de la cámara
            Vector3 cameraPos = new Vector3(
                _cameraDistance * (float)Math.Sin(_cameraAngleY) * (float)Math.Cos(_cameraAngleX),
                _cameraDistance * (float)Math.Sin(_cameraAngleX),
                _cameraDistance * (float)Math.Cos(_cameraAngleY) * (float)Math.Cos(_cameraAngleX)
            );

            _view = Matrix4.LookAt(cameraPos, Vector3.Zero, Vector3.UnitY); // La cámara mira al centro

            // Pasar matrices al shader
            GL.UniformMatrix4(GL.GetUniformLocation(_shaderProgram, "view"), false, ref _view);
            GL.UniformMatrix4(GL.GetUniformLocation(_shaderProgram, "projection"), false, ref _projection);

            // Dibujar objetos (sin rotar, solo su posición inicial)
            foreach (var obj in objetos)
            {
                obj.Render(_shaderProgram);
            }

            SwapBuffers();
        }

        static void Main(string[] args)
        {
            List<Objeto3D> L1 = new List<Objeto3D>();
            float[] uVertices = { 
                //ATRAS
                -0.8f, -1.0f, -0.5f, 1.0f, 1.0f, 1.0f, //arista 
                -0.8f,  1.0f, -0.5f, 1.0f, 1.0f, 1.0f,

                 0.8f, -1.0f, -0.5f, 1.0f, 1.0f, 1.0f, //arista 
                 0.8f,  1.0f, -0.5f, 1.0f, 1.0f, 1.0f,

                -0.8f, -1.0f, -0.5f, 1.0f, 1.0f, 1.0f, //arista 
                 0.8f, -1.0f, -0.5f, 1.0f, 1.0f, 1.0f,

                -0.3f,  -0.5f, -0.5f, 1.0f, 1.0f, 1.0f, //arista 
                 0.3f,  -0.5f, -0.5f, 1.0f, 1.0f, 1.0f,

                -0.3f,   1.0f, -0.5f, 1.0f, 1.0f, 1.0f, //arista  
                -0.3f,  -0.5f, -0.5f, 1.0f, 1.0f, 1.0f,

                 0.3f,   1.0f, -0.5f, 1.0f, 1.0f, 1.0f, //arista  
                 0.3f,  -0.5f, -0.5f, 1.0f, 1.0f, 1.0f,

                -0.8f,   1.0f, -0.5f, 1.0f, 1.0f, 1.0f, //arista  
                -0.3f,   1.0f, -0.5f, 1.0f, 1.0f, 1.0f,

                 0.8f,   1.0f, -0.5f, 1.0f, 1.0f, 1.0f, //arista  
                 0.3f,   1.0f, -0.5f, 1.0f, 1.0f, 1.0f,

            //FRENTE
                -0.8f, -1.0f, 0.5f, 1.0f, 1.0f, 1.0f, //arista 
                -0.8f,  1.0f, 0.5f, 1.0f, 1.0f, 1.0f,

                 0.8f, -1.0f, 0.5f, 1.0f, 1.0f, 1.0f, //arista 
                 0.8f,  1.0f, 0.5f, 1.0f, 1.0f, 1.0f,

                -0.8f, -1.0f, 0.5f, 1.0f, 1.0f, 1.0f, //arista 
                 0.8f, -1.0f, 0.5f, 1.0f, 1.0f, 1.0f,

                -0.3f,  -0.5f, 0.5f, 1.0f, 1.0f, 1.0f, //arista 
                 0.3f,  -0.5f, 0.5f, 1.0f, 1.0f, 1.0f,

                -0.3f,   1.0f, 0.5f, 1.0f, 1.0f, 1.0f, //arista  
                -0.3f,  -0.5f, 0.5f, 1.0f, 1.0f, 1.0f,

                 0.3f,   1.0f, 0.5f, 1.0f, 1.0f, 1.0f, //arista  
                 0.3f,  -0.5f, 0.5f, 1.0f, 1.0f, 1.0f,

                -0.8f,   1.0f, 0.5f, 1.0f, 1.0f, 1.0f, //arista  
                -0.3f,   1.0f, 0.5f, 1.0f, 1.0f, 1.0f,

                 0.8f,   1.0f, 0.5f, 1.0f, 1.0f, 1.0f, //arista  
                 0.3f,   1.0f, 0.5f, 1.0f, 1.0f, 1.0f,

            //CONEXIONES ENTRE FRENTE Y ATRAS
                -0.8f,  1.0f, -0.5f, 1.0f, 1.0f, 1.0f, //arista 
                -0.8f,  1.0f,  0.5f, 1.0f, 1.0f, 1.0f,

                 0.8f,  1.0f, -0.5f, 1.0f, 1.0f, 1.0f, //arista 
                 0.8f,  1.0f,  0.5f, 1.0f, 1.0f, 1.0f,

                -0.8f, -1.0f, -0.5f, 1.0f, 1.0f, 1.0f, //arista 
                -0.8f, -1.0f,  0.5f, 1.0f, 1.0f, 1.0f,

                 0.8f, -1.0f, -0.5f, 1.0f, 1.0f, 1.0f, //arista 
                 0.8f, -1.0f,  0.5f, 1.0f, 1.0f, 1.0f,

                -0.3f, -0.5f, -0.5f, 1.0f, 1.0f, 1.0f, //arista  
                -0.3f, -0.5f, 0.5f, 1.0f, 1.0f, 1.0f,

                 0.3f, -0.5f, -0.5f, 1.0f, 1.0f, 1.0f, //arista  
                 0.3f, -0.5f, 0.5f, 1.0f, 1.0f, 1.0f,

                -0.3f, 1.0f, -0.5f, 1.0f, 1.0f, 1.0f, //arista  
                -0.3f, 1.0f,  0.5f, 1.0f, 1.0f, 1.0f,

                 0.3f, 1.0f, -0.5f, 1.0f, 1.0f, 1.0f, //arista  
                 0.3f, 1.0f,  0.5f, 1.0f, 1.0f, 1.0f,
                                    };
            float[] ejesXYZ = { 
                //eje x
                -5.0f, 0.0f,  0.0f, 0.0f, 1.0f, 0.0f,
                 5.0f, 0.0f,  0.0f, 0.0f, 1.0f, 0.0f,

                //eje y
                0.0f,  -5.0f,  0.0f, 0.0f, 0.0f, 1.0f,
                0.0f,   5.0f,  0.0f, 0.0f, 0.0f, 1.0f,
                
                //eje z
                0.0f,  0.0f,  -5.0f, 1.0f, 0.0f, 0.0f,
                0.0f,  0.0f,   5.0f, 1.0f, 0.0f, 0.0f,
                                        };
            float[] cuboVertices = {
                -0.5f, -0.5f,  0.5f, 1.0f, 0.0f, 0.0f, // Inferior izquierdo, rojo
                -0.5f,  0.5f,  0.5f, 0.0f, 1.0f, 0.0f, // Superior izquierdo, verde

                -0.5f,  0.5f,  0.5f, 0.0f, 1.0f, 0.0f, // Superior izquierdo, verde
                0.5f,  0.5f,  0.5f, 0.0f, 0.0f, 1.0f, // Superior derecho, azul

                0.5f,  0.5f,  0.5f, 0.0f, 0.0f, 1.0f, // Superior derecho, azul
                0.5f, -0.5f,  0.5f, 1.0f, 0.647f, 0.0f, // Inferior derecho, naranja

                0.5f, -0.5f,  0.5f, 1.0f, 0.647f, 0.0f, // Inferior derecho, naranja
                -0.5f, -0.5f,  0.5f, 1.0f, 0.0f, 0.0f, // Inferior izquierdo, rojo

                // Atrás
                -0.5f, -0.5f, -0.5f, 1.0f, 0.0f, 0.0f, // Inferior izquierdo, rojo
                -0.5f,  0.5f, -0.5f, 0.0f, 1.0f, 0.0f, // Superior izquierdo, verde

                -0.5f,  0.5f, -0.5f, 0.0f, 1.0f, 0.0f, // Superior izquierdo, verde
                0.5f,  0.5f, -0.5f, 0.0f, 0.0f, 1.0f, // Superior derecho, azul

                0.5f,  0.5f, -0.5f, 0.0f, 0.0f, 1.0f, // Superior derecho, azul
                0.5f, -0.5f, -0.5f, 1.0f, 0.647f, 0.0f, // Inferior derecho, naranja

                0.5f, -0.5f, -0.5f, 1.0f, 0.647f, 0.0f, // Inferior derecho, naranja
                -0.5f, -0.5f, -0.5f, 1.0f, 0.0f, 0.0f, // Inferior izquierdo, rojo


                // Conexiones entre frente y atrás
                -0.5f, -0.5f,  0.5f, 1.0f, 0.0f, 0.0f, // Inferior izquierdo, rojo (frente)
                -0.5f, -0.5f, -0.5f, 1.0f, 0.0f, 0.0f, // Inferior izquierdo, rojo (atrás)

                0.5f, -0.5f,  0.5f, 1.0f, 0.647f, 0.0f, // Inferior derecho, naranja (frente)
                0.5f, -0.5f, -0.5f, 1.0f, 0.647f, 0.0f, // Inferior derecho, naranja (atrás)

                -0.5f,  0.5f,  0.5f, 0.0f, 1.0f, 0.0f, // Superior izquierdo, verde (frente)
                -0.5f,  0.5f, -0.5f, 0.0f, 1.0f, 0.0f, // Superior izquierdo, verde (atrás)

                0.5f,  0.5f,  0.5f, 0.0f, 0.0f, 1.0f, // Superior derecho, azul (frente)
                0.5f,  0.5f, -0.5f, 0.0f, 0.0f, 1.0f, // Superior derecho, azul (atrás)

            };

            L1.Add(new Objeto3D(uVertices, -2f, -2f, -2f));
            L1.Add(new Objeto3D(ejesXYZ, 0f, 0f, 0f));
            L1.Add(new Objeto3D(cuboVertices, 3f, 2f, 5f));

            var nativeWindowSettings = new NativeWindowSettings()
            {
                ClientSize = new Vector2i(800, 600),
                Title = "Cubo 3D con OpenTK y Shaders",
                Flags = ContextFlags.Default,
                Profile = ContextProfile.Core,
            };

            using (var window = new Program(GameWindowSettings.Default, nativeWindowSettings, L1))
            {
                window.Run();
            }
        }
    }
}