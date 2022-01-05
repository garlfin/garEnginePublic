using System.Drawing;
using System.Drawing.Imaging;
using garEngine.ecs_sys.component;
using garEngine.ecs_sys.system;
using garEngine.render.model;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;


namespace garEngine.render;

public static class WorldSettings
{
   public static Vector3 LightPos = new(10,10,10);
   public static int cubeMapTexID;
   public static int vao;
   public static int vbo;
   private static Material skyboxMaterial;
   public static Material ShadowDepthMaterial;
   public static Material DepthMaterial;


   private static int _fbo;
   public static int _texture;
   private static Matrix4 _lightProjection, _lightView;
   public static Matrix4 lightSpaceMatrix;

   static WorldSettings()
   {
   }

   public static void ShadowBuffer(int width, int height)
   {
      _fbo = GL.GenFramebuffer();
      _texture = GL.GenTexture();
      GL.BindTexture(TextureTarget.Texture2D, _texture);
      GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent, width, height, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
      GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
      GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
      GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
      GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);
      float[] borderColor = {1.0f, 1.0f, 1.0f, 1.0f};
      GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBorderColor, borderColor);
        
      GL.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);
      GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, _texture, 0);
      GL.DrawBuffer(DrawBufferMode.None);
      GL.ReadBuffer(ReadBufferMode.None);
      GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
   }

   public static void RenderShadow()
   {
      float _nearPlane = 1.0f, _farPlane = 100f;
      _lightProjection = Matrix4.CreateOrthographicOffCenter(-100.0f, 100.0f, -100.0f, 100.0f, _nearPlane, _farPlane);
      _lightView = Matrix4.LookAt(WorldSettings.LightPos, Vector3.Zero, Vector3.UnitY);
      lightSpaceMatrix = _lightView * _lightProjection;
      GL.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);
      GL.Clear(ClearBufferMask.DepthBufferBit);
      ModelRendererSystem.UpdateDepth(true);
      GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        
   }
   public static void SetShadowDepthMaterial(Material material)
   {
       ShadowDepthMaterial = material;
   }
   public static void SetDepthMaterial(Material material)
   {
      DepthMaterial = material;
   }
   

   public static void genVao()
   {

      AssimpLoaderTest.MeshStruct cubeObject = new AssimpLoaderTest("../../../resources/model/cube.obj").getMesh(0);
      
      
      vao = GL.GenVertexArray();
      vbo = GL.GenBuffer();
      GL.BindVertexArray(vao);
      GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
      GL.BufferData(BufferTarget.ArrayBuffer, cubeObject.points.Count * 3 * sizeof(float), cubeObject.points.ToArray(), BufferUsageHint.StaticDraw);
      GL.EnableVertexAttribArray(0);
      GL.VertexAttribPointer(0,3,VertexAttribPointerType.Float, false, 0, 0);
   }

   public static void renderSkybox()
   {
      GL.CullFace(CullFaceMode.Front);
      GL.DepthFunc(DepthFunction.Lequal);
      GL.BindVertexArray(vao);
      GL.ActiveTexture(TextureUnit.Texture0);
      GL.BindTexture(TextureTarget.TextureCubeMap, cubeMapTexID);
      skyboxMaterial.Use();
      Matrix4 viewMatrix = RenderView._camera.GetViewMatrix().ClearTranslation();
      skyboxMaterial.SetUniform("view", ref viewMatrix);
      Matrix4 projectionMatrix = RenderView._camera.GetProjectionMatrix();
      skyboxMaterial.SetUniform("projection", ref projectionMatrix);
      skyboxMaterial.SetUniform("skybox", 0);
      GL.DrawArrays(PrimitiveType.Triangles, 0,36);
      GL.DepthFunc(DepthFunction.Less);
      GL.CullFace(CullFaceMode.Back);
   }
   public static List<string> PathHelper(List<string> pathList)
   {
      List<string> returnList = new List<string>();
      foreach (string x in pathList)
      {
         returnList.Add("../../../resources/texture/cubemap/"+x+".jpg");
      }
      return returnList;
   }

   public static void SetSkyboxMaterial(Material material)
   {
      skyboxMaterial = material;
   }   
   public static void LoadCubemap(List<string> path)
   {
      List<TextureTarget> targets = new List<TextureTarget>()
      {
         TextureTarget.TextureCubeMapNegativeX,
         TextureTarget.TextureCubeMapNegativeY,
         TextureTarget.TextureCubeMapNegativeZ,
         TextureTarget.TextureCubeMapPositiveX,
         TextureTarget.TextureCubeMapPositiveY,
         TextureTarget.TextureCubeMapPositiveZ
      };
      cubeMapTexID = GL.GenTexture();
      GL.BindTexture(TextureTarget.TextureCubeMap, cubeMapTexID);
      for (int i = 0; i < 6; i++)
      {
         Bitmap bmp = new Bitmap(path[i]);
         BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly,
            System.Drawing.Imaging.PixelFormat.Format24bppRgb);
         GL.TexImage2D(targets[i], 0, PixelInternalFormat.Rgb8, bmp.Width, bmp.Height, 0, PixelFormat.Bgr, PixelType.UnsignedByte, bmpData.Scan0 );
         
         bmp.UnlockBits(bmpData);
         bmp.Dispose();
      }
      GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
      GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
      GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int) TextureWrapMode.ClampToEdge);
      GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int) TextureWrapMode.ClampToEdge);
      GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int) TextureWrapMode.ClampToEdge);
   }

   public static void Delete()
   {
      GL.DeleteTexture(cubeMapTexID);
      GL.DeleteBuffer(vbo);
      GL.DeleteVertexArray(vao);
   }
}