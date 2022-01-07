#version  330 core

out vec4 FragColor;

in vec2 texCoords;

uniform sampler2D screenTexture;
uniform sampler2D normalTexture;
uniform sampler2D positionTexture;

void main()
{
    FragColor = texture(screenTexture, texCoords);
}