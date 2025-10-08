# GAM531-openTk-a3_3DCube

1. Which library you used (SharpDX or OpenTK).
   
   with OpenTk.
   
   
2. How I rendered the cube.

   
   I first created 36 vertices to support all face of my cube (3 vertices * 2 triangle per face * 6 faces) and assignemnt color to each square face (2 triangle face).

   added vec3 aColor color to my vertex and fragment shader as course slide showed.

   and added perspective and view matrices to position the cube in 3D space
   
   ```
   
      Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(
      MathHelper.DegreesToRadians(45f),
      (float)Size.X / Size.Y,
      0.1f, 100f
      );

      Matrix4 view = Matrix4.CreateTranslation(0f, 0f, -3f);
      Matrix4 model = Matrix4.CreateRotationY(rotationAngles) * Matrix4.CreateRotationX(rotationAngles * 0.5f);
      Matrix4 mvp = model * view * projection;

      GL.UniformMatrix4(GL.GetUniformLocation(shaderProgramHandle, "uMVP"), false, ref mvp);
   
   ```



     than finally draw it out using

   ```

      GL.BindVertexArray(vertexArrayHandle);
      GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
      GL.BindVertexArray(0);

   ```

   
--
## Screenshot

![Running Program](./Screenshot%202025-09-23%20174852.png)
