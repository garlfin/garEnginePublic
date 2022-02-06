#version 330 core

layout (location = 0) in vec2 inPos;
layout (location = 2) in vec2 inTexCoords;

out vec2 TexCoord;

void main()
{
    gl_Position = vec4(inPos.x, inPos.y, 0.0, 1.0);
    TexCoord = inTexCoords;
}
    #FRAGMENT
    #version  330 core

out vec4 FragColor;

in vec2 TexCoord;




uniform sampler2D screenTexture;
uniform sampler2D ao;

void main()
{


    const float gamma = 2.2;
    const float exposure = 1;
    

    int   size       = 5;
    float separation = 2;
    float threshold  = 1;
    float amount     = 0.6;

    vec2 texSize = textureSize(screenTexture, 0).xy;

    vec4 result = vec4(0.0);
    vec4 color  = vec4(0.0);

    float value = 0.0;
    float count = 0.0;

    for (int i = -size; i <= size; ++i) {
        for (int j = -size; j <= size; ++j) {
            color =
            texture
            ( screenTexture
            ,   (vec2(i, j) * separation + gl_FragCoord.xy)
            / texSize
            );

            // exposure tone mapping

            value = max(color.r, max(color.g, color.b));
            if (value < threshold) { color = vec4(0.0); }

            result += color;
            count  += 1.0;
        }
    }

    result /= count;
    vec4 ScreenTex = texture(screenTexture, TexCoord);
    vec3 hdrColor = ScreenTex.rgb;

    // exposure tone mapping
    vec3 mapped = vec3(1.0) - exp(-hdrColor * exposure);
    // gamma correction 
    mapped = pow(mapped, vec3(1.0 / gamma));
    float aoData = texture(ao,TexCoord).r;
    FragColor = vec4(clamp(mapped + vec3(result),0,1) * mix(1,aoData, ScreenTex.w), 1.0);
}