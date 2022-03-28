#version 330

layout(location = 0) in vec3 vPosition;

uniform mat4 projection;
uniform mat4 view;
uniform mat4 model;

out vec4 FragPos;

void main() {
    gl_Position = vec4(vPosition, 1.0) * model * view * projection;
    FragPos = vec4(vPosition, 1.0) * model;
}

-FRAGMENT-
#version 330 core
in vec4 FragPos;

uniform vec3 lightPos;
uniform float far;

void main()
{

    float lightDistance = length(FragPos.xyz - lightPos);
    lightDistance = lightDistance / far;
    gl_FragDepth = lightDistance;
}  