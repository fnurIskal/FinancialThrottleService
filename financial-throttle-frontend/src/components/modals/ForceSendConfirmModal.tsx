import { AlertTriangle } from 'lucide-react';
import type { SuspendedGroup } from '../../types';
import Modal from '../ui/Modal';

interface Props {
  group: SuspendedGroup | null;
  onClose: () => void;
  onConfirm: () => void;
  loading?: boolean;
}

export default function ForceSendConfirmModal({ group, onClose, onConfirm, loading }: Props) {
  if (!group) return null;

  return (
    <Modal
      open={!!group}
      onClose={onClose}
      title="Confirm Force Send"
      size="sm"
      footer={
        <>
          <button onClick={onClose} disabled={loading} className="btn-secondary">
            Cancel
          </button>
          <button onClick={onConfirm} disabled={loading} className="btn-danger">
            {loading ? 'Sending...' : 'Force Send'}
          </button>
        </>
      }
    >
      <div className="space-y-4">
        <div className="flex items-start gap-3 bg-red-50 border border-red-200 rounded-xl px-4 py-3">
          <AlertTriangle size={18} className="text-red-500 flex-shrink-0 mt-0.5" />
          <p className="text-sm text-red-700">
            This will skip all validation and send directly.{' '}
            <strong>This action cannot be undone.</strong>
          </p>
        </div>

        <div className="grid grid-cols-1 gap-2">
          {[
            ['Security', group.securityCode],
            ['Database', group.databaseName],
            ['Template ID', `#${group.templateId}`],
          ].map(([label, value]) => (
            <div key={label} className="bg-gray-50 rounded-lg px-3 py-2.5 flex justify-between items-center">
              <span className="text-xs text-gray-400">{label}</span>
              <span className="text-sm font-semibold text-gray-900">{value}</span>
            </div>
          ))}
        </div>
      </div>
    </Modal>
  );
}
