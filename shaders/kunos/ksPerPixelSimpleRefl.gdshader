shader_type spatial;
render_mode blend_mix, depth_draw_opaque, cull_back, diffuse_burley;

uniform float ksAmbient = 1.0;
uniform float ksDiffuse = 1.0;
uniform float ksSpecular = 1.0;
uniform float ksSpecularEXP = 1.0;
uniform float ksEmissive = 1.0;
uniform float ksAlphaRef = 1.0;
uniform float fresnelC = 0.0; 
uniform float fresnelEXP = 0.0; 
uniform float isAdditive = 0.0; 
uniform float fresnelMaxLevel = 0.0; 

uniform sampler2D txDiffuse : source_color, filter_linear_mipmap, repeat_enable;

void fragment() {
	vec2 base_uv = UV;

	vec4 diffuse = texture(txDiffuse, base_uv);
	ALBEDO = diffuse.rgb;
	//ALBEDO = albedo_tex.rgb;

	//float metallic_tex = dot(texture(texture_metallic, base_uv), metallic_texture_channel);
	SPECULAR = ksSpecular;
	METALLIC = 1.0-ksSpecular;
	ROUGHNESS = 1.0-ksSpecular;
	
	//vec4 roughness_texture_channel = vec4(1.0, 0.0, 0.0, 0.0);
	//float roughness_tex = dot(texture(texture_roughness, base_uv), roughness_texture_channel);
	//ROUGHNESS = roughness_tex * roughness;
	
	//NORMAL_MAP = texture(texture_normal, base_uv).rgb;
	//NORMAL_MAP_DEPTH = normal_scale;
	
	//ALPHA *= albedo.a * albedo_tex.a;
	//ALPHA_SCISSOR_THRESHOLD = 0.5;
}

//void light() {
//	// Called for every pixel for every light affecting the material.
//	// Uncomment to replace the default light processing function with this one.
//}
