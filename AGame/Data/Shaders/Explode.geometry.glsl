layout(triangles) in;
layout(triangle_strip, max_vertices = 9) out;

void map(int i) {
	///MAPPINGS
}

vec4 Pos(int i) {
	return gl_in[i].gl_Position;
}

void main() {
	vec4 V = Pos(1) - Pos(0);
	vec4 W = Pos(2) - Pos(0);
	vec3 Norm = -normalize(vec3((V.y * W.z) - (V.z * W.y), (V.z * W.x) - (V.x * W.z), (V.x * W.y) - (V.y * W.x)));

	for(int i = 0; i < 3; i++) {
		map(i);
		vec4 Pos = Pos(i);
		float s = clamp(sin(Time * 0.8) * 4, 0.0, 3.0) * 5;
		gl_Position = vec4(Pos.xyz + Norm * s, Pos.w);
		EmitVertex();
	}
	EndPrimitive();
}