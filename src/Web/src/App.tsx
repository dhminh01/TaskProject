import "antd/dist/reset.css";
import { BrowserRouter } from "react-router-dom";
import { AppContent } from "./components/layout/AppContent";
import { ProConfigProvider } from "@ant-design/pro-components";

export default function App() {
  return (
    <ProConfigProvider>
      <BrowserRouter>
        <AppContent />
      </BrowserRouter>
    </ProConfigProvider>
  );
}
