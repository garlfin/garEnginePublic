#version 330 core

layout (location = 0) in vec2 inPos;
layout (location = 2) in vec2 inTexCoords;

void main()
{
    gl_Position = vec4(inPos.x, inPos.y, 0.0, 1.0);
}
    //-FRAGMENT-
    #version 330 core

uniform sampler2D positionTexture;
uniform sampler2D colorTexture;

uniform mat4 prevView;
uniform mat4 view;
uniform mat4 projection;

uniform int doBlur;

out vec4 fragColor;

void main() {
    int samples = 5;

    vec2 texSize = textureSize(colorTexture, 0).xy;
    vec2 texCoord = gl_FragCoord.xy / texSize;

    fragColor = texture(colorTexture, texCoord);
    
    if (doBlur == 0) return;
    
    vec4 prevPos = vec4(texture(positionTexture, texCoord).rgb, 1.0);

    vec4 currentPos = view * prevView * prevPos;
    currentPos = projection * currentPos;
    currentPos /= currentPos.w;

    prevPos = projection * prevPos;
    prevPos /= prevPos.w;
    vec2 direction = (prevPos.xy - currentPos.xy) * 0.025;

    if (length(direction) <= 0.0) return;
    for (int i = 0; i < samples; ++i, texCoord -= direction) fragColor += texture(colorTexture, texCoord);

    fragColor /= samples + 1;
}