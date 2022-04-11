#version 330 core

layout (location = 0) in vec3 inPos;
layout (location = 1) in vec3 inTexCoords;

out vec2 TexCoord;

uniform mat4 model;

void main()
{
    gl_Position = vec4(inPos.x, inPos.y, 0.0, 1.0) * model;
    TexCoord = inTexCoords.xy;
}

    //-FRAGMENT-
    #version 330 core
in vec2 TexCoord;
out vec4 FragColor;

uniform sampler2D font;

void main() {
    float opacity = smoothstep(0, 1, texture(font, TexCoord).r);
    FragColor = vec4(1.0, 1.0, 1.0, opacity);
}
