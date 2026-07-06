import { StyleSheet, Text, View } from 'react-native';
import { colors } from '../constants/colors';

export type BadgeVariant = 'success' | 'waiting' | 'failure' | 'info' | 'neutral';

const VARIANT_STYLES: Record<BadgeVariant, { bg: string; text: string }> = {
  success: { bg: colors.success, text: '#166534' },
  waiting: { bg: colors.waiting, text: '#9A3412' },
  failure: { bg: colors.failure, text: '#991B1B' },
  info: { bg: colors.info, text: '#1E40AF' },
  neutral: { bg: '#E5E7EB', text: colors.textSecondary },
};

interface BadgeProps {
  variant: BadgeVariant;
  children: React.ReactNode;
}

export default function Badge({ variant, children }: BadgeProps) {
  const style = VARIANT_STYLES[variant];
  return (
    <View style={[styles.badge, { backgroundColor: style.bg }]}>
      <Text style={[styles.text, { color: style.text }]}>{children}</Text>
    </View>
  );
}

const styles = StyleSheet.create({
  badge: {
    paddingHorizontal: 8,
    paddingVertical: 3,
    borderRadius: 999,
    alignSelf: 'flex-start',
  },
  text: {
    fontSize: 11,
    fontWeight: '600',
  },
});
