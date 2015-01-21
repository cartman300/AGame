out vec4 Color;

vec4 Lighting(vec4 Ambient, vec3 LightPos, vec3 Norm, mat4 NormMat) {
	float LStren = clamp(dot((NormMat * vec4(Norm, 1.0)).xyz, LightPos), 0.0, 1.0);
	return max(Ambient, vec4(LStren, LStren, LStren, 1.0));
}

void main() {
	vec4 TexClr = texture(Texture, __uv) * Lighting(vec4(0.2, 0.2, 0.2, 1), vec3(0, 1, 0), __norm, NormMatrix);

	if (MultColor == 0)
		Color = TexClr + ObjColor;
	else
		Color = TexClr * ObjColor;
}