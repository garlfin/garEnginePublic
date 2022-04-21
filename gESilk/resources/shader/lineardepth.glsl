#version 330
#extension GL_NV_viewport_array2 : enable
#extension GL_NV_viewport_array : enable
#extension GL_ARB_shader_viewport_layer_array : enable
#extension GL_AMD_vertex_shader_layer : enable


layout(location = 0) in vec3 vPosition;

uniform mat4 model;
uniform mat4 shadowMatrices[6];

out vec4 FragPos;

void main() {
    FragPos = vec4(vPosition, 1.0) * model;
    gl_Position = vec4(vPosition, 1.0) * model * shadowMatrices[gl_InstanceID];
    gl_Layer = gl_InstanceID;
}

    //-FRAGMENT-
    #version 330 core
in vec4 FragPos;

uniform vec3 lightPos;

void main()
{
    float lightDistance = length(FragPos.xyz - lightPos);
    lightDistance = lightDistance / 100.0;
    gl_FragDepth = lightDistance;
}