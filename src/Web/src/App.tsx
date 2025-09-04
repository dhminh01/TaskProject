import "antd/dist/reset.css";
import { BrowserRouter } from "react-router-dom";
import { AppContent } from "./components/AppContent";

export default function App() {
  return (
    <BrowserRouter>
      <AppContent />
    </BrowserRouter>
  );
}
