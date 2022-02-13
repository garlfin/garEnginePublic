#version 330 core
layout (location = 0) in vec3 aPos;
layout(location = 1) in vec3 vNormal;

out vec3 TexCoords;
out vec3 viewFragPos;
out vec3 viewNormal;


uniform mat4 projection;
uniform mat4 view;
uniform mat4 model;


void main()
{
    TexCoords = aPos;
    vec4 pos = vec4(aPos, 1.0) * model * view * projection;
    viewFragPos = vec3(vec4(aPos, 1.0) * model * view);
    gl_Position = pos.xyww;
    viewNormal = normalize(vNormal * mat3(transpose(inverse(model*view))));
}
    
    #FRAGMENT
    
    #version 330 core

layout (location = 0) out vec4 FragColor;
layout (location = 1) out vec3 FragNormal;
layout (location = 2) out vec3 FragLoc;

in vec3 TexCoords;
in vec3 viewFragPos;
in vec3 viewNormal;

uniform samplerCube skybox;

void main()
{
    FragColor = vec4(vec3(texture(skybox, TexCoords))*1.1,0);
    FragNormal = viewNormal;
    FragLoc = viewFragPos;
    
}