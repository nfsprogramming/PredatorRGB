import 'package:flutter/material.dart';
import '../models/lighting_profile.dart';
import '../widgets/keyboard_preview.dart';
import '../theme/app_theme.dart';
import '../services/tcp_service.dart';

class ProfilesScreen extends StatefulWidget {
  final TcpService tcpService;

  const ProfilesScreen({super.key, required this.tcpService});

  @override
  State<ProfilesScreen> createState() => _ProfilesScreenState();
}

class _ProfilesScreenState extends State<ProfilesScreen> {
  final List<LightingProfile> _profiles = const [
    LightingProfile(
      id: 'gaming',
      name: 'Gaming',
      effectName: 'Cyber Wave',
      effectType: LightEffect.wave,
      tcpCommand: 'MODE:WAVE',
    ),
    LightingProfile(
      id: 'coding',
      name: 'Coding',
      effectName: 'Static Gradient',
      effectType: LightEffect.static, // Could use aura or static
      tcpCommand: 'MODE:STATIC',
    ),
    LightingProfile(
      id: 'office',
      name: 'Office',
      effectName: 'Breathing',
      effectType: LightEffect.breathing,
      tcpCommand: 'MODE:BREATHING',
    ),
    LightingProfile(
      id: 'night',
      name: 'Night',
      effectName: 'Aurora',
      effectType: LightEffect.aura,
      tcpCommand: 'MODE:AURA',
    ),
    LightingProfile(
      id: 'movie',
      name: 'Movie',
      effectName: 'Fire',
      effectType: LightEffect.fire,
      tcpCommand: 'MODE:FIRE',
    ),
  ];

  String _activeProfileId = 'gaming';

  void _onProfileTapped(LightingProfile profile) {
    setState(() {
      _activeProfileId = profile.id;
    });
    widget.tcpService.sendCommand(profile.tcpCommand);
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Colors.transparent,
      body: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Header
          Padding(
            padding: const EdgeInsets.fromLTRB(32, 32, 32, 16),
            child: Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      'PROFILES',
                      style: Theme.of(context).textTheme.titleLarge?.copyWith(
                            color: Colors.white,
                            letterSpacing: 1.2,
                            fontWeight: FontWeight.bold,
                          ),
                    ),
                    const SizedBox(height: 4),
                    Container(
                      height: 2,
                      width: 60,
                      color: AppTheme.primary,
                    ),
                  ],
                ),
                TextButton(
                  onPressed: () {},
                  child: Text(
                    'Create New',
                    style: TextStyle(
                      color: AppTheme.primary,
                      fontSize: 16,
                      fontWeight: FontWeight.w500,
                    ),
                  ),
                ),
              ],
            ),
          ),
          
          // List of profiles
          Expanded(
            child: ListView.builder(
              padding: const EdgeInsets.symmetric(horizontal: 32),
              itemCount: _profiles.length,
              itemBuilder: (context, index) {
                return _buildProfileCard(_profiles[index]);
              },
            ),
          ),
          
          // Bottom Actions
          Container(
            padding: const EdgeInsets.symmetric(vertical: 24),
            decoration: BoxDecoration(
              border: Border(
                top: BorderSide(color: Colors.white.withOpacity(0.05)),
              ),
            ),
            child: Row(
              mainAxisAlignment: MainAxisAlignment.spaceEvenly,
              children: [
                TextButton.icon(
                  onPressed: () {},
                  icon: const Icon(Icons.upload_rounded, color: Colors.white70),
                  label: const Text('Import', style: TextStyle(color: Colors.white70)),
                ),
                Container(
                  width: 1,
                  height: 24,
                  color: Colors.white.withOpacity(0.1),
                ),
                TextButton.icon(
                  onPressed: () {},
                  icon: const Icon(Icons.download_rounded, color: Colors.white70),
                  label: const Text('Export', style: TextStyle(color: Colors.white70)),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildProfileCard(LightingProfile profile) {
    final isActive = profile.id == _activeProfileId;
    
    // Determine colors based on effect for the icon
    Color profileColor;
    IconData profileIcon;
    switch (profile.id) {
      case 'gaming':
        profileColor = Colors.redAccent;
        profileIcon = Icons.sports_esports;
        break;
      case 'coding':
        profileColor = Colors.blueAccent;
        profileIcon = Icons.code;
        break;
      case 'office':
        profileColor = Colors.tealAccent;
        profileIcon = Icons.work;
        break;
      case 'night':
        profileColor = Colors.indigoAccent;
        profileIcon = Icons.nights_stay;
        break;
      case 'movie':
        profileColor = Colors.orangeAccent;
        profileIcon = Icons.movie;
        break;
      default:
        profileColor = Colors.white;
        profileIcon = Icons.palette;
    }

    return GestureDetector(
      onTap: () => _onProfileTapped(profile),
      child: Container(
        margin: const EdgeInsets.only(bottom: 12),
        padding: const EdgeInsets.all(12),
        decoration: BoxDecoration(
          color: isActive 
              ? AppTheme.primary.withOpacity(0.15) 
              : AppTheme.surfaceContainer,
          borderRadius: BorderRadius.circular(12),
          border: Border.all(
            color: isActive ? AppTheme.primary : Colors.white.withOpacity(0.05),
            width: isActive ? 1.5 : 1,
          ),
          boxShadow: isActive ? [
            BoxShadow(
              color: AppTheme.primary.withOpacity(0.2),
              blurRadius: 12,
              spreadRadius: 2,
            )
          ] : [],
        ),
        child: Row(
          children: [
            // Icon / Graphic Placeholder
            Container(
              width: 80,
              height: 50,
              decoration: BoxDecoration(
                color: Colors.black.withOpacity(0.3),
                borderRadius: BorderRadius.circular(8),
                border: Border.all(color: profileColor.withOpacity(0.3)),
              ),
              child: Center(
                child: Icon(profileIcon, color: profileColor, size: 28),
              ),
            ),
            const SizedBox(width: 16),
            
            // Text Info
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    profile.name,
                    style: const TextStyle(
                      color: Colors.white,
                      fontSize: 16,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                  const SizedBox(height: 4),
                  Text(
                    profile.effectName,
                    style: TextStyle(
                      color: Colors.white.withOpacity(0.6),
                      fontSize: 13,
                    ),
                  ),
                ],
              ),
            ),
            
            // Mini Keyboard Preview
            SizedBox(
              width: 120,
              height: 50,
              child: ClipRRect(
                borderRadius: BorderRadius.circular(4),
                child: FittedBox(
                  fit: BoxFit.contain,
                  alignment: Alignment.center,
                  child: SizedBox(
                    width: 400, // Using arbitrary large size which is scaled down by FittedBox
                    height: 120, // To maintain aspect ratio for KeyboardPreview
                    child: IgnorePointer(
                      child: KeyboardPreview(
                        effect: profile.effectType,
                        baseColor: profileColor, // pass color if supported
                        speed: 1.0,
                        brightness: 0.8,
                      ),
                    ),
                  ),
                ),
              ),
            ),
            
            const SizedBox(width: 16),
            
            // More menu
            Icon(
              Icons.more_vert,
              color: Colors.white.withOpacity(0.5),
            ),
          ],
        ),
      ),
    );
  }
}
