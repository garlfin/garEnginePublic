#version 330 core
layout (location = 0) in vec3 aPos;
layout(location = 1) in vec3 vNormal;

out vec3 TexCoords;



uniform mat4 projection;
uniform mat4 view;
uniform mat4 model;


void main()
{
    TexCoords = aPos;
    vec4 pos = vec4(aPos, 1.0) * model * view * projection;
    gl_Position = pos.xyww;
}

-FRAGMENT-

#version 330 core

layout (location = 0) out vec4 FragColor;
layout (location = 1) out vec3 FragNormal;
layout (location = 2) out vec4 FragLoc;

in vec3 TexCoords;

uniform sampler2D equirectangularMap;

const vec2 invAtan = vec2(0.1591, 0.3183);

vec2 SampleSphericalMap(vec3 v)
{
    vec2 uv = vec2(atan(v.z, v.x), asin(v.y));
    uv *= invAtan;
    uv += 0.5;
    return uv;
}


void main()
{
    vec2 uv = SampleSphericalMap(normalize(TexCoords));
    vec3 color = texture(equirectangularMap, uv).rgb;

    //color = vec3(1.0) - exp(-color * 1); 
    FragColor = vec4(color, 1.0);
}

