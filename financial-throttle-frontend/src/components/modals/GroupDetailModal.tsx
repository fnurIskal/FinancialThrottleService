import type { WaitingGroup } from '../../types';
import Modal from '../ui/Modal';
import Badge from '../ui/Badge';

interface Props {
  group: WaitingGroup | null;
  onClose: () => void;
}

export default function GroupDetailModal({ group, onClose }: Props) {
  if (!group) return null;

  return (
    <Modal
      open={!!group}
      onClose={onClose}
      title={`Group: ${group.databaseName} | ${group.securityCode} | Template #${group.templateId}`}
      size="lg"
      footer={
        <button onClick={onClose} className="btn-secondary">Close</button>
      }
    >
      <div className="space-y-5">
        {/* General info */}
        <div>
          <h3 className="text-xs font-bold text-gray-400 uppercase tracking-wider mb-3">General Info</h3>
          <div className="grid grid-cols-2 gap-3">
            {[
              ['Database', group.databaseName],
              ['Security ID', group.securityId],
              ['Security Code', group.securityCode],
              ['Template ID', `#${group.templateId}`],
              ['Item Count', group.itemCount],
            ].map(([label, value]) => (
              <div key={String(label)} className="bg-gray-50 rounded-lg p-3">
                <div className="text-xs text-gray-400">{label}</div>
                <div className="text-sm font-semibold text-gray-900 mt-0.5">{String(value)}</div>
              </div>
            ))}
          </div>
        </div>

        {/* Items */}
        <div>
          <h3 className="text-xs font-bold text-gray-400 uppercase tracking-wider mb-3">
            Items ({group.items.length})
          </h3>
          <div className="space-y-2">
            {group.items.map((item, i) => (
              <div key={i} className="flex items-center gap-3 bg-gray-50 rounded-lg px-3 py-2.5">
                <div className="flex-1">
                  <div className="flex items-center gap-2">
                    <span className="text-sm font-medium text-gray-900">
                      Q{item.quarter}
                    </span>
                    {item.isOriginal && (
                      <Badge variant="info" className="text-xs">Original *</Badge>
                    )}
                  </div>
                  <div className="text-xs text-gray-400 mt-0.5">
                    Disclosure #{item.disclosureId} · {item.username}
                  </div>
                  <div className="mt-1">
                    <span className="text-gray-400 text-xs">Table Type: </span>
                    <span className="text-gray-900 text-xs font-medium">
                      {item.tableTypeId === 1 ? "1 — Quarterly"
                       : item.tableTypeId === 2 ? "2 — TTM"
                       : item.tableTypeId === 3 ? "3 — Footnotes"
                       : item.tableTypeId === 4 ? "4 — Attachments"
                       : `${item.tableTypeId}`}
                    </span>
                  </div>
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>
    </Modal>
  );
}
