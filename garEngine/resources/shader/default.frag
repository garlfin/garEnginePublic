#version 330

uniform sampler2D albedo;

in vec3 FragPos;
in vec2 fTexCoord;
in vec3 fNormal;

out vec4 colorOut;

void main() {

    vec3 lightDir = normalize(vec3(10,10,10) - FragPos);  
    float ambient = max(0,dot(lightDir,fNormal))*0.5+0.5;
    //colorOut = vec4(1*ambient);
    colorOut = vec4(fTexCoord,1,1);
}