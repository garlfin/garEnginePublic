#version 330 core

uniform sampler2D albedo;

in vec2 TexCoord;

void main()
{             
    // gl_FragDepth = gl_FragCoord.z;
    float alpha = texture(albedo, TexCoord).a;
    if (alpha < 0.5) {
        discard;
    }
}