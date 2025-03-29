using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

class Objeto3D
{
    private int _vao, _vbo;
    private float[] _vertices;
    private Matrix4 _modelo;

    private float cx;
    private float cy;
    private float cz;

    //constructor
    public Objeto3D(float[] vertices,float centroX, float centroY, float centroZ)
    {
        _vertices = vertices;
        cx = centroX;
        cy = centroY;
        cz = centroZ;
        actualizarVertices();
        _modelo = Matrix4.Identity; // Matriz de identidad (sin transformación)
    }
    public void Inicializar()
    {
        _vao = GL.GenVertexArray();
        _vbo = GL.GenBuffer();

        GL.BindVertexArray(_vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
        GL.EnableVertexAttribArray(1);

        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        GL.BindVertexArray(0);
    }

    public void actualizarVertices(){
        for (int i = 0; i < _vertices.Length; i += 6)
        {
            _vertices[i] += cx;  // x
            _vertices[i + 1] += cy;  // y
            _vertices[i + 2] += cz;  // z
        }
    }

    public void SetTransform(Matrix4 transform)
    {
        _modelo = transform;
    }

    public void Render(int shaderProgram)
    {
        GL.UseProgram(shaderProgram);
        GL.BindVertexArray(_vao);

        int modelLocation = GL.GetUniformLocation(shaderProgram, "model");
        GL.UniformMatrix4(modelLocation, false, ref _modelo);

        GL.DrawArrays(PrimitiveType.Lines, 0, 72);
        GL.BindVertexArray(0);
    }
}
