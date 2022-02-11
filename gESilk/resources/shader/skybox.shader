#version 330 core
layout (location = 0) in vec3 aPos;

out vec3 TexCoords;
out vec3 viewFragPos;


uniform mat4 projection;
uniform mat4 view;
uniform mat4 model;


void main()
{
    TexCoords = aPos;
    vec4 pos = vec4(aPos, 1.0) * model * view * projection;
    viewFragPos = vec3(vec4(aPos, 1.0) * model * view);
    gl_Position = pos.xyww;
}
    
    #FRAGMENT
    
    #version 330 core

layout (location = 0) out vec4 FragColor;
layout (location = 1) out vec3 FragNormal;
layout (location = 2) out vec3 FragLoc;

in vec3 TexCoords;
in vec3 viewFragPos;

uniform samplerCube skybox;

void main()
{
    FragColor = vec4(vec3(texture(skybox, TexCoords)),0)*1.1;
    FragNormal = vec3(0,0,1);
    FragLoc = viewFragPos;
    
}