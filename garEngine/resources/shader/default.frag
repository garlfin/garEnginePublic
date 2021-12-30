#version 330

uniform sampler2D albedo;
uniform samplerCube cubemap;

in vec3 FragPos;
in vec2 fTexCoord;
in mat3 TBN;
in vec3 tangent;

in vec3 fViewVec;
in vec3 fLightPos;
uniform sampler2D normalMap;

out vec4 colorOut;

vec4 mixMultiply(vec4 inone, vec4 intwo, float mixfac){
    return mix(inone, inone*intwo, mixfac);
}

void main() {
    vec4 color = vec4(1); 
    float specFactor = 1 - 0.75;
    vec3 normal = texture(normalMap, fTexCoord).rgb;
    normal = vec3(normal.r, 1-normal.g, normal.b) * 2.0 - 1.0;
    vec3 lightDir = normalize(fLightPos-FragPos);  
    float ambient = max(dot(lightDir,normal),0)*0.5+0.5;
    vec3 viewPos = normalize(fViewVec-FragPos);
    float fresnel = clamp(1 - max(dot(viewPos, normal),0),0,1);
    // light reflected off normal, dot product with view vector
    float spec = clamp(pow(max(0,dot(reflect(lightDir, normal),-viewPos)),pow(12,1+specFactor)),0,1)*specFactor; 
    color = texture(albedo, fTexCoord)*ambient+vec4(spec);
    color = mixMultiply(color, texture(cubemap, reflect(-viewPos, normal)), fresnel * specFactor);
    color = pow(color, vec4(1/2.2));
    //colorOut = color;
    colorOut = vec4(tangent,1);
    //colorOut = texture(cubemap, reflect(-viewPos, normal));
}