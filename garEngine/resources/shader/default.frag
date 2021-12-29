#version 330

uniform sampler2D albedo;

in vec3 FragPos;
in vec2 fTexCoord;
in vec3 fNormal;

uniform vec3 viewVec;

out vec4 colorOut;

void main() {
    float specFactor = 1 - 0.75;
    vec3 lightDir = normalize(vec3(10,10,10) - FragPos);  
    float ambient = max(0,dot(lightDir,fNormal))*0.5+0.5;
    // light reflected off normal, dot product with view vector
    float spec = clamp(pow(max(0,dot(reflect(lightDir, fNormal),-normalize(viewVec-FragPos))),pow(12,1+specFactor)),0,1)*specFactor; 
    colorOut = texture(albedo, fTexCoord)*ambient+vec4(spec);
}