out vec4 Color;

void main() {
	float FntA = texture(Texture, __uv).a;
	vec4 Clr = vec4(vec3(1.0, 1.0, 1.0), FntA) * __clr;

	Color = Clr * ObjColor;
}