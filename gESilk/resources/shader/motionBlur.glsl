#version 330 core

layout (location = 0) in vec2 inPos;
layout (location = 2) in vec2 inTexCoords;

void main()
{
    gl_Position = vec4(inPos.x, inPos.y, 0.0, 1.0);
}
    //-FRAGMENT-
    /*
      (C) 2020 David Lettier
      lettier.com
    */
    #version 330 core

uniform sampler2D positionTexture;
uniform sampler2D colorTexture;

uniform mat4 prevView;
uniform mat4 view;
uniform mat4 projection;

out vec4 fragColor;

void main() {
    int   size       = 5;
    float separation =     0.025;

    vec2 texSize  = textureSize(colorTexture, 0).xy;
    vec2 texCoord = gl_FragCoord.xy / texSize;

    fragColor = texture(colorTexture, texCoord);
    vec4 position1 = texture(positionTexture, texCoord);

    vec4 position0 = view * prevView * position1;
    position0      = projection * position0;
    position0.xy   = position0.xy * 0.5 + 0.5;

    position1      = projection * position1;
    position1.xy   = position1.xy * 0.5 + 0.5;

    vec2 direction = position1.xy - position0.xy;

    if (length(direction) <= 0.0) { return; }

    direction.xy *= separation;

    vec2 forward  = texCoord;
    vec2 backward = texCoord;

    for (int i = 0; i < size; ++i) {
        backward -= direction;
        fragColor += texture(colorTexture, backward);
    }

    fragColor /= size + 1;
}