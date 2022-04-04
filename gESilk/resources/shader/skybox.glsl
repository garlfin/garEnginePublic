#version 330 core
layout (location = 0) in vec3 aPos;
layout(location = 1) in vec3 vNormal;

out vec3 TexCoords;
out vec3 ViewFragPos;


uniform mat4 projection;
uniform mat4 view;
uniform mat4 model;


void main()
{
    TexCoords = aPos;
    vec4 pos = vec4(aPos, 1.0) * model * view * projection;
    gl_Position = pos.xyww;
    ViewFragPos = vec3(vec4(aPos, 1.0) * view);
}

-FRAGMENT-

#version 330 core

layout (location = 0) out vec4 FragColor;
layout (location = 1) out vec3 FragNormal;
layout (location = 2) out vec4 FragLoc;

in vec3 TexCoords;
in vec3 ViewFragPos;

uniform samplerCube skybox;

void main()
{
    vec3 color = texture(skybox, TexCoords).rgb;
    
    FragColor = vec4(color, 0.0);
    FragNormal = vec3(1.0);
    FragLoc = vec4(1.0);
}