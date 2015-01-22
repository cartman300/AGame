out vec4 Color;

uniform mat4 Matrix3D;

vec4 FXAA(sampler2D, vec2, vec2);

float GetVoxel(vec3 V) {
	return texture(VoxWorld, V).r;
}

#define StepLen 0.05

void main() {
	vec3 Fwd = normalize((Matrix3D * vec4(__uv, -1, 1)).xyz);

	/*Color = vec4(0, 0, 0, 1);

	for (int i = 0; i < 100; i++) {
		if (GetVoxel(Fwd + (Fwd * StepLen * i)) > 100) {
			Color = vec4(vec3(1, 0, 0) * (1 - (float(i) / 100)), 1);
			break;
		}
	}*/

	Color = FXAA(Texture, __uv, Resolution) * texture(VoxWorld, Fwd * 10);
}