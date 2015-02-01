out vec4 Color;
//out vec4 NormalColor;
 
vec4 Lighting(vec4 Ambient, vec3 LightPos, vec3 Norm, mat4 NormMat) {
	float LStren = clamp(dot((NormMat * vec4(Norm, 1.0)).xyz, LightPos), 0.0, 1.0);
	return max(Ambient, vec4(LStren, LStren, LStren, 1.0));
}

void main() {
	vec4 TexClr = texture(Texture, __uv);
	//TexClr = vec4(1, 1, 1, 1);
	TexClr *= Lighting(vec4(0.2, 0.2, 0.2, 1), vec3(0, 1, 0), __norm, NormMatrix);

	Color = TexClr * ObjColor;
	//Color = vec4(__norm.x * 0.5 + 0.5, __norm.y * 0.5 + 0.5, __norm.z * 0.5 + 0.5, 1.0);
}